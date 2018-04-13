using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Olive.Entities;
using Olive.Entities.Data;

namespace Olive.Mvc.Testing
{
    public class TestDatabaseGenerator
    {
        static object SyncLock = new object();

        readonly string ConnectionString;
        string TempDatabaseName;
        DatabaseManager Server;

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

        /// <summary>
        /// Creates a new TestDatabaseGenerator instance.
        /// </summary>
        public TestDatabaseGenerator()
        {
            ConnectionString = Config.GetConnectionString("AppDatabase");
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

        void CreateDatabaseFromScripts()
        {
            Server.ClearConnectionPool();
            Server.Delete(TempDatabaseName);

            foreach (var file in GetExecutableCreateDbScripts())
            {
                try { Server.Execute(file.Value); }
                catch (Exception ex)
                { throw new Exception("Could not execute sql file '" + file.Key.FullName + "'", ex); }
            }
            Server.ClearConnectionPool();
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

                if (file.Name.Lacks("Create.Database.sql", caseSensitive: false))
                    script = "USE [" + TempDatabaseName + "];\r\nGO\r\n" + script;

                result.Add(file, script);
            }

            return result;
        }

        public void Process(WebTestConfig config)
        {
            Server = config.DatabaseServer;
            LoadMetaDirectory();

            TempDatabaseName = DatabaseManager.GetDatabaseName().Or("Default.Temp");
            if (!TempDatabaseName.ToLower().EndsWith(".temp"))
            {
                Debug.WriteLine($"Temp databae creation aborted as '{TempDatabaseName}' does not end in '.Temp'.");
                return;
            }

            EnsurePermissions();
            CreateDatabaseFilesPath();

            lock (SyncLock)
            {
                CreateDatabaseFromScripts();
                CopyFiles();
            }

            Task.Factory.RunSync(() => Database.Instance.Refresh());
        }

        /// <summary>
        /// Ensures the right permissions are configured.
        /// </summary>
        void EnsurePermissions()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;

            Debug.WriteLine($"Temp databae creation: current identity '{identity}' has enough permission.");

            var error = "\r\n\r\nRecommended action: If using IIS, update the Application Pool (Advanced Settings) and set Identity to LocalSystem.";

            if (identity.IsEmpty())
            {
                error = "Current IIS process model Identity not found!" + error;
                throw new Exception(error);
            }
            else
            {
                error = "Current IIS process model Identity: " + identity + error;
            }

            if (identity.ContainsAny(new[] { "IIS APPPOOL", "LOCAL SERVICE", "NETWORK SERVICE" }))
            {
                error = "In TDD mode full system access is needed in order to create temporary database files." + error;
                throw new Exception(error);
            }

            Debug.WriteLine($"Temp databae creation: '{identity}' seems good to use.");
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