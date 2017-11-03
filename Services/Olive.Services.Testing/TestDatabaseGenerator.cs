using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Olive.Entities.Data;

namespace Olive.Services.Testing
{
    public class TestDatabaseGenerator
    {
        const string TEMP_DATABASES_LOCATION_KEY = "Temp.Databases.Location";

        static AsyncLock AsyncLock = new AsyncLock();
        static AsyncLock ProcessAsyncLock = new AsyncLock();

        readonly string ConnectionString;
        SqlServerManager MasterDatabaseAgent;
        string TempDatabaseName, ReferenceDatabaseName;

        FileInfo ReferenceMDFFile, ReferenceLDFFile;
        DirectoryInfo TempBackupsRoot, ProjectTempRoot, DbDirectory, CurrentHashDirectory;

        readonly bool IsTempDatabaseOptional, MustRenew;

        public bool CreatedNewDatabase { get; private set; }

        /// <summary>
        /// Creates a new TestDatabaseGenerator instance.
        /// <param name="isTempDatabaseOptional">Determines whether use of the temp database is optional.
        /// When this class is used in a Unit Test project, then it must be set to false.
        /// For Website project, it must be set to true.</param>
        /// <param name="mustRenew">Specifies whether the temp database must be recreated on application start up even if it looks valid already.</param>
        /// </summary>
        public TestDatabaseGenerator(bool isTempDatabaseOptional, bool mustRenew)
        {
            ConnectionString = Config.GetConnectionString("AppDatabase");

            IsTempDatabaseOptional = isTempDatabaseOptional;

            MustRenew = mustRenew;
        }

        FileInfo[] GetCreateDbFiles()
        {
            if (DbDirectory == null)
                LoadMSharpMetaDirectory();

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
                throw new Exception("No SQL creation script file was found. I checked:\r\n" + potentialSources.ToLinesString());

            return sources;
        }

        async Task<Dictionary<FileInfo, string>> GetExecutableCreateDbScripts()
        {
            var sources = GetCreateDbFiles();

            var result = new Dictionary<FileInfo, string>();

            foreach (var file in sources)
            {
                var script = await file.ReadAllText();

                // The first few lines contain #DATABASE.NAME# which should be replaced.
                script = script.ToLines().Select((line, index) =>
                {
                    if (index < 10)
                    {
                        return line
                            .Replace("#DATABASE.NAME#", ReferenceDatabaseName)
                            .Replace("#STORAGE.PATH#", CurrentHashDirectory.FullName);
                    }

                    return line;
                }).ToLinesString();

                if (file.Name.Lacks("Create.Database.sql", caseSensitive: false))
                {
                    script = "USE [" + ReferenceDatabaseName + "];\r\nGO\r\n" + script;
                }

                result.Add(file, script);
            }

            return result;
        }

        internal async Task<string> GetCurrentDatabaseCreationHash()
        {
            var createScript = (await GetCreateDbFiles().Select(async x => await x.ReadAllText()).AwaitAll()).ToLinesString();

            return createScript.ToSimplifiedSHA1Hash();
        }

        async Task CreateDatabaseFromScripts()
        {
            await MasterDatabaseAgent.DeleteDatabase(ReferenceDatabaseName);

            var newDatabaseAgent = MasterDatabaseAgent.CloneFor(ReferenceDatabaseName);

            foreach (var file in await GetExecutableCreateDbScripts())
            {
                try
                {
                    await MasterDatabaseAgent.ExecuteSql(file.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not execute sql file '" + file.Key.FullName + "' becuase '" + ex.Message + "'", ex);
                }
            }
        }

        public async Task CloneReferenceDatabaseToTemp()
        {
            // Make sure if it exists in database already, it's deleted first.
            await MasterDatabaseAgent.DeleteDatabase(TempDatabaseName);

            var directory = ProjectTempRoot.GetOrCreateSubDirectory("Current");

            var newMDFPath = directory.GetFile(TempDatabaseName + ".mdf");
            var newLDFPath = directory.GetFile(TempDatabaseName + "_log.ldf");

            try
            {
                await ReferenceMDFFile.CopyTo(newMDFPath);
                await ReferenceLDFFile.CopyTo(newLDFPath);
            }
            catch (IOException ex)
            {
                if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
                    throw new Exception("Consider setting the IIS Application Pool identity to LocalSystem.", ex);

                throw;
            }

            var script = "CREATE DATABASE [{0}] ON (FILENAME = '{1}'), (FILENAME = '{2}') FOR ATTACH"
                .FormatWith(TempDatabaseName, newMDFPath.FullName, newLDFPath.FullName);

            try
            {
                await MasterDatabaseAgent.ExecuteSql(script);
            }
            catch (SqlException ex)
            {
                throw new Exception("Could not attach the database from file " + newMDFPath.FullName + "." + Environment.NewLine +
                "Hint: Ensure SQL instance service has access to the folder. E.g. 'Local Service' may not have access to '{0}'" +
                newMDFPath.Directory.FullName, ex);
            }
        }

        internal async Task TryAccessNewTempDatabase()
        {
            Exception error = null;
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    await Database.Instance.GetAccess().ExecuteQuery("SELECT TABLE_NAME FROM [{0}].INFORMATION_SCHEMA.TABLES".FormatWith(TempDatabaseName));
                    return;
                }
                catch (Exception ex)
                {
                    SqlConnection.ClearAllPools();
                    error = ex;
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
            }

            throw new Exception("Could not access the new database:" + error.Message, error);
        }

        public async Task<bool> Process()
        {
            if (ConnectionString.IsEmpty()) return false;

            var builder = new SqlConnectionStringBuilder(ConnectionString);
            TempDatabaseName = builder.InitialCatalog.Or("").TrimStart("[").TrimEnd("]");

            if (TempDatabaseName.IsEmpty())
            {
                // None of my business.
                return false;
            }
            else if (!TempDatabaseName.ToLower().EndsWith(".temp") && IsTempDatabaseOptional)
            {
                // Optional and irrelevant
                return false;
            }

            EnsurePermissions();

            builder.InitialCatalog = "master";

            MasterDatabaseAgent = new SqlServerManager(builder.ToString());

            LoadTempDatabaseLocation();
            LoadMSharpMetaDirectory();

            if (!IsTempDatabaseOptional)
            {
                if (!IsExplicitlyTempDatabase())
                {
                    throw new Exception("For unit tests project the database name must end in '.Temp'.");
                }
            }

            if (!IsExplicitlyTempDatabase())
            {
                // Not Temp mode:
                return false;
            }

            return await DoProcess();
        }

        /// <summary>
        /// Ensures the right permissions are configured.
        /// </summary>
        void EnsurePermissions()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;

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
        }

        void LoadTempDatabaseLocation()
        {
            var specifiedLocation = Config.Get(TEMP_DATABASES_LOCATION_KEY);

            if (specifiedLocation.IsEmpty())
            {
                throw new Exception("You must specify a valid path for AppSetting of '{0}'.".FormatWith(TEMP_DATABASES_LOCATION_KEY));
            }

            if (!specifiedLocation.AsDirectory().Exists())
            {
                // Try to build once:
                try
                {
                    Directory.CreateDirectory(specifiedLocation);
                }
                catch
                {
                    throw new Exception("Could not create the folder '{0}'. Ensure it exists and is accessible. Otherwise specify a different location in AppSetting of '{1}'."
                        .FormatWith(specifiedLocation, TEMP_DATABASES_LOCATION_KEY));
                }
            }

            TempBackupsRoot = specifiedLocation.AsDirectory();
            ProjectTempRoot = TempBackupsRoot.GetOrCreateSubDirectory(TempDatabaseName);
        }

        void LoadMSharpMetaDirectory()
        {
            // Not explicitly specified. Take a guess:
            var folder = AppDomain.CurrentDomain.BaseDirectory.AsDirectory().Parent;
            while (folder.Parent != null)
            {
                DbDirectory = folder.GetSubDirectory("DB");
                if (DbDirectory.Exists()) return;

                folder = folder.Parent;
            }

            throw new Exception("Failed to find the DB folder from which to create the temp database.");
        }

        async Task<bool> DoProcess()
        {
            var hash = (await GetCurrentDatabaseCreationHash()).Replace("/", "-").Replace("\\", "-");

            using (await AsyncLock.Lock())
            {
                ReferenceDatabaseName = TempDatabaseName + ".Ref";

                CurrentHashDirectory = ProjectTempRoot.GetOrCreateSubDirectory(hash);
                ReferenceMDFFile = CurrentHashDirectory.GetFile(ReferenceDatabaseName + ".mdf");
                ReferenceLDFFile = CurrentHashDirectory.GetFile(ReferenceDatabaseName + "_log.ldf");

                using (await ProcessAsyncLock.Lock())
                {
                    var createdNewReference = await CreateReferenceDatabase();

                    var tempDatabaseDoesntExist = !await MasterDatabaseAgent.DatabaseExists(TempDatabaseName);

                    if (MustRenew || createdNewReference || tempDatabaseDoesntExist)
                    {
                        await RefreshTempDataWorld();
                    }
                }

                return true;
            }
        }

        async Task RefreshTempDataWorld()
        {
            await CloneReferenceDatabaseToTemp();

            SqlConnection.ClearAllPools();

            await CopyFiles();

            // Do we really need this?
            await TryAccessNewTempDatabase();

            CreatedNewDatabase = true;
        }

        async Task<bool> CreateReferenceDatabase()
        {
            if (ReferenceMDFFile.Exists() && ReferenceLDFFile.Exists())
            {
                return false;
            }

            var error = false;

            // create database + data
            try
            {
                await CreateDatabaseFromScripts();
            }
            catch
            {
                error = true;
                throw;
            }
            finally
            {
                // Detach it
                await MasterDatabaseAgent.DetachDatabase(ReferenceDatabaseName);

                if (error)
                {
                    await ReferenceMDFFile.Delete(harshly: true);
                    await ReferenceLDFFile.Delete(harshly: true);
                }
            }

            return true;
        }

        bool IsExplicitlyTempDatabase() => TempDatabaseName.ToLower().EndsWith(".temp");

        public async Task CleanUp() => await MasterDatabaseAgent.DeleteDatabase(TempDatabaseName);

        async Task CopyFiles()
        {
            var copyTasks = new List<Task>();

            foreach (
                var key in
                    new[]
                        {
                            Tuple.Create("Test.Files.Origin:Open", "UploadFolder"),
                            Tuple.Create("Test.Files.Origin:Secure", "UploadFolder.Secure")
                        })
            {
                var source = Config.Get(key.Item1);
                if (source.IsEmpty()) continue;
                else source = AppDomain.CurrentDomain.GetPath(source);
                if (!Directory.Exists(source) || source.AsDirectory().GetDirectories().None())
                {
                    // No files to copy
                    continue;
                }

                var destination = Config.Get(key.Item2);
                if (destination.IsEmpty())
                    throw new Exception("Destination directory not configured in App.Config for key: " + key.Item2);
                else destination = AppDomain.CurrentDomain.GetPath(destination);

                if (!Directory.Exists(destination))
                {
                    if (new DirectoryInfo(source).IsEmpty()) continue;

                    Directory.CreateDirectory(destination);
                }

                await new DirectoryInfo(destination).Clear();

                copyTasks.Add(new DirectoryInfo(source).CopyTo(destination, overwrite: true));
            }

            await Task.WhenAll(copyTasks);
        }
    }
}