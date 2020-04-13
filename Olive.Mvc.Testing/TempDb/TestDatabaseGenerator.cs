using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    public class TestDatabaseGenerator
    {
        static object SyncLock = new object();

        readonly string ConnectionString;
        readonly IDatabaseServer DatabaseManager;
        string TempDatabaseName;
        internal bool DropExisting;

        DirectoryInfo DbDirectory, DatabaseFilesPath;

        public static DirectoryInfo DatabaseStoragePath
        {
            get
            {
                var result = Config.GetOrThrow("Database:StoragePath").AsDirectory();

                if (!result.Exists())
                {
                    // Try to build once:
                    try { result.Create(); }
                    catch
                    {
                        throw new Exception($"Failed to create the folder '{result.FullName}'. " +
                            "Ensure it exists and is accessible to the current ASP.NET process. " +
                            $"Otherwise specify a different location in AppSetting of 'Database:StoragePath'.");
                    }
                }

                return result;
            }
        }

        public TestDatabaseGenerator(IDatabaseServer databaseManager)
        {
            ConnectionString = Context.Current
                .GetService<IConnectionStringProvider>()
                .GetConnectionString("Default");
            DatabaseManager = databaseManager;
        }

        FileInfo[] GetCreateDbFiles()
        {
            if (DbDirectory == null) LoadMetaDirectory();

            var potentialSources = new List<FileInfo>();

            var tableScripts = DbDirectory.GetSubDirectory("Tables").GetFilesOrEmpty("*.sql");

            // Create tables:
            potentialSources.Add(DbDirectory.GetFile("@Create.Database.sql"));
            potentialSources.AddRange(tableScripts.Except(x => x.Name.ToLower().EndsWithAny(".fk.sql", ".data.sql")));

            // Insert data:
            potentialSources.Add(DbDirectory.GetFile("@Create.Database.Data.sql"));
            potentialSources.AddRange(tableScripts.Where(x => x.Name.ToLower().EndsWith(".data.sql")));

            potentialSources.Add(DbDirectory.GetFile("Customize.Database.sql"));

            // Add foreign keys            
            potentialSources.AddRange(tableScripts.Where(x => x.Name.ToLower().EndsWith(".fk.sql")));

            var sources = potentialSources.Where(f => f.Exists()).ToArray();

            if (sources.None())
                throw new Exception("Failed to find the SQL creation script files at:\r\n" + potentialSources.ToLinesString());

            return sources;
        }

        void CreateDatabaseFilesPath()
        {
            var createScript = GetCreateDbFiles().Select(f => File.ReadAllText(f.FullName)).ToLinesString();

            var hash = createScript.ToSimplifiedSHA1Hash().Replace("/", "-").Replace("\\", "-");
            Debug.WriteLine("Temp database: Hash of the current DB scripts -> " + hash);

            DatabaseFilesPath = DatabaseStoragePath.GetOrCreateSubDirectory(TempDatabaseName).GetOrCreateSubDirectory(hash);
        }

        bool EstablishDatabaseFromScripts()
        {
            if (!DropExisting)
                if (DatabaseManager.Exists(TempDatabaseName, DatabaseFilesPath.FullName))
                    return false;

            CreateDatabaseFromScripts();
            return true;
        }

        void CreateDatabaseFromScripts()
        {
            DatabaseManager.ClearConnectionPool();
            DatabaseManager.Delete(TempDatabaseName);

            foreach (var file in GetExecutableCreateDbScripts())
            {
                try { DatabaseManager.Execute(file.Value, TempDatabaseName); }
                catch (Exception ex)
                { throw new Exception("Could not execute sql file '" + file.Key.FullName + "'", ex); }
            }

            DatabaseManager.ClearConnectionPool();
        }

        Dictionary<FileInfo, string> GetExecutableCreateDbScripts()
        {
            var sources = GetCreateDbFiles();

            var result = new Dictionary<FileInfo, string>();

            foreach (var file in sources)
            {
                var script = File.ReadAllText(file.FullName);

                // The first few lines contain #DATABASE.NAME# which should be replaced.
                script = script.ToLines().Select((line, index) =>
                {
                    if (index < 10)
                    {
                        return line
                            .Replace("#DATABASE.NAME#", TempDatabaseName)
                            .Replace("#STORAGE.PATH#", DatabaseFilesPath.FullName);
                    }

                    return line;
                }).ToLinesString();

                result.Add(file, script);
            }

            return result;
        }

        public bool Process()
        {
            LoadMetaDirectory();

            TempDatabaseName = DatabaseManager.GetDatabaseName().Or("Default.Temp");
            if (!TempDatabaseName.ToLower().EndsWith(".temp"))
            {
                Debug.WriteLine($"Temp databae creation aborted as '{TempDatabaseName}' does not end in '.Temp'.");
                return false;
            }

            CreateDatabaseFilesPath();

            lock (SyncLock)
            {
                if (!EstablishDatabaseFromScripts()) return false;
                CopyFiles();
            }

            Task.Factory.RunSync(() => Context.Current.Database().Refresh());

            return true;
        }

        void LoadMetaDirectory()
        {
            DbDirectory = AppDomain.CurrentDomain.WebsiteRoot().Parent.GetSubDirectory("DB");
            if (!DbDirectory.Exists())
                throw new Exception("Failed to find the DB folder from which to create the temp database: " +
                    DbDirectory.FullName);
        }

        void CopyFiles()
        {
            DiskBlobStorageProvider.Root.DeleteIfExists(recursive: true);

            var path = Config.Get("Blob:WebTest:Origin", "..\\Test\\ReferenceFiles");

            var source = AppDomain.CurrentDomain.WebsiteRoot().GetSubDirectory(path);
            if (source.Exists())
                source.CopyTo(DiskBlobStorageProvider.Root.FullName, overwrite: true);
        }
    }
}