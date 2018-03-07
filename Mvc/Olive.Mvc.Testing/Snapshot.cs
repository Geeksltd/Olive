using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Entities;
using Olive.Entities.Data;

namespace Olive.Mvc.Testing
{
    class Snapshot
    {
        const string URL_FILE_NAME = "url.txt", DATE_FILE_NAME = "date.txt";
        static string DatabaseName = GetDatabaseName();
        string SnapshotName;
        bool IsInShareSnapshotMode;
        static Mutex SnapshotRestoreLock;

        DirectoryInfo SnapshotsDirectory;

        public Snapshot(string name, bool isSharedSNapshotMode)
        {
            IsInShareSnapshotMode = isSharedSNapshotMode;
            SnapshotName = CreateSnapshotName(name);
            SnapshotsDirectory = GetSnapshotsRoot(IsInShareSnapshotMode).GetSubDirectory(SnapshotName);
        }

        public async Task Create()
        {
            if (!Enabled) return;

            SetupDirecory();
            await SnapshotDatabase();
            await CreateSnapshotCookies();
            DiskBlobStorageProvider.GetRoot(SnapshotsDirectory)
                    .CopyTo(DiskBlobStorageProvider.Root.FullName, overwrite: true);
            await SaveDate();
            await SaveUrl();
        }

        static bool Enabled => Config.Get("Olive.Mvc.Testing:Snapshot:Enabled", defaultValue: true);

        public bool Exists()
        {
            if (!Enabled) return false;
            return SnapshotsDirectory.Exists();
        }

        public async Task Restore()
        {
            if (!Exists())
                throw new DirectoryNotFoundException("Cannot find snapshot " + SnapshotName);

            var restoreDatabase = LocalTime.Now;
            await RestoreDatabase();
            Debug.WriteLine("Total time for restoring including mutex: " + LocalTime.Now.Subtract(restoreDatabase).Milliseconds);

            var restoreCookies = LocalTime.Now;
            await RestoreCookies();
            Debug.WriteLine("Total time for restoring cookies: " + LocalTime.Now.Subtract(restoreCookies).Milliseconds);

            var restoreFiles = LocalTime.Now;
            DiskBlobStorageProvider.GetRoot(SnapshotsDirectory)
                .CopyTo(DiskBlobStorageProvider.Root.FullName, overwrite: true);

            Debug.WriteLine("Total time for restoring files: " + LocalTime.Now.Subtract(restoreFiles).Milliseconds);

            var restoreDate = LocalTime.Now;
            await RestoreDate();
            Debug.WriteLine("Total time for restoring date: " + LocalTime.Now.Subtract(restoreDate).Milliseconds);

            var restoreUrl = LocalTime.Now;
            await RestoreUrl();
            Debug.WriteLine("Total time for restoring url: " + LocalTime.Now.Subtract(restoreUrl).Milliseconds);
        }

        async Task SaveDate()
        {
            if (LocalTime.IsRedefined)
            {
                await File.WriteAllTextAsync(SnapshotsDirectory.GetFile(DATE_FILE_NAME).FullName, LocalTime.Now.ToString());
            }
        }

        async Task RestoreDate()
        {
            var dateFile = SnapshotsDirectory.GetFile(DATE_FILE_NAME);
            if (dateFile.Exists())
            {
                var dateTime = Convert.ToDateTime(await dateFile.ReadAllTextAsync());
                LocalTime.RedefineNow(() => dateTime);
            }
        }

        public static Task RemoveSnapshots()
        {
            var sharedSnapshots = GetSnapshotsRoot(isSharedSnapshotMode: true);
            if (sharedSnapshots.Exists)
            {
                DeleteDirectory(sharedSnapshots);
                sharedSnapshots.EnsureExists();
            }

            var normalSnapshots = GetSnapshotsRoot(isSharedSnapshotMode: false);
            if (normalSnapshots.Exists)
            {
                DeleteDirectory(normalSnapshots);
                normalSnapshots.EnsureExists();
            }

            Context.Current.Response().Redirect("~/");
            return Task.CompletedTask;
        }

        public static Task RemoveSnapshot(string name)
        {
            var snapshotName = CreateSnapshotName(name);

            var normalSnapshotDirectory = Path.Combine(GetSnapshotsRoot(isSharedSnapshotMode: false).FullName, snapshotName).AsDirectory();
            if (normalSnapshotDirectory.Exists)
                DeleteDirectory(normalSnapshotDirectory);

            var shardSnapshotDirectory = Path.Combine(GetSnapshotsRoot(isSharedSnapshotMode: true).FullName, snapshotName).AsDirectory();
            if (shardSnapshotDirectory.Exists)
                DeleteDirectory(shardSnapshotDirectory);

            Context.Current.Response().Redirect("~/");
            return Task.CompletedTask;
        }

        public static void DeleteDirectory(DirectoryInfo targetDirectory)
        {
            var files = targetDirectory.GetFiles();
            var dirs = targetDirectory.GetDirectories();

            foreach (var file in files)
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete(harshly: true);
            }

            foreach (var dir in dirs)
                DeleteDirectory(dir);

            targetDirectory.DeleteIfExists(recursive: true);
        }

        #region URL

        async Task SaveUrl()
        {
            var uri = new Uri(Context.Current.Request().ToAbsoluteUri());
            var url = uri.PathAndQuery;

            url = url.Substring(0, url.IndexOf("Web.Test.Command", StringComparison.OrdinalIgnoreCase) - 1);
            if (url.HasValue())
            {
                await File.WriteAllTextAsync(SnapshotsDirectory.GetFile(URL_FILE_NAME).FullName, url);
                Context.Current.Response().Redirect(url);
            }
        }

        async Task RestoreUrl()
        {
            var urlFile = SnapshotsDirectory.GetFile(URL_FILE_NAME);
            if (urlFile.Exists())
                Context.Current.Response().Redirect(Context.Current.Request().RootUrl() + (await urlFile.ReadAllTextAsync()).TrimStart("/"));
        }

        #endregion

        #region Cookie
        async Task CreateSnapshotCookies()
        {
            var json = JsonConvert.SerializeObject(Context.Current.Request().GetCookies().ToArray());

            await GetCookiesFile().WriteAllTextAsync(json);
        }

        async Task RestoreCookies()
        {
            var cookiesFile = GetCookiesFile();

            if (!cookiesFile.Exists()) return;

            var cookies = JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>(await cookiesFile.ReadAllTextAsync());

            foreach (var cookie in cookies)
                Context.Current.Response().Cookies.Append(cookie.Key, cookie.Value);
        }

        FileInfo GetCookiesFile() => SnapshotsDirectory.GetFile("cookies.json");

        #endregion

        #region DB
        async Task SnapshotDatabase()
        {
            FileInfo[] files;

            SqlConnection.ClearAllPools();

            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();
                files = await GetPhysicalFiles(connection);

                await TakeDatabaseOffline(connection);
                await files.Do(async f =>
                {
                    if (IsInShareSnapshotMode)
                    {
                        await f.CopyToAsync(Path.Combine(SnapshotsDirectory.FullName, GetSnapshotFileName(f) + f.Extension).AsFile());

                        // keep the snashptname of the database in a .origin file
                        await File.WriteAllTextAsync(SnapshotsDirectory.GetFile(
                            GetSnapshotFileName(f) + f.Extension + ".origin").FullName,
                            f.FullName.Replace(DatabaseName, GetSnapshotFileName(f)));
                    }
                    else
                    {
                        await f.CopyTo(SnapshotsDirectory);
                        // keep the original location of the database file in a .origin file
                        await File.WriteAllTextAsync(SnapshotsDirectory.GetFile(f.Name + ".origin").FullName, f.FullName);
                    }
                });
                await TakeDatabaseOnline(connection);
            }
        }

        string GetSnapshotFileName(FileInfo file) => file.Name.Split('.').First() + ".Temp";

        // TODO: create a connection string for MASTER
        async Task RestoreDatabase()
        {
            SnapshotRestoreLock = new Mutex(false, "SnapshotRestore");
            var lockTaken = false;

            try
            {
                lockTaken = SnapshotRestoreLock.WaitOne();
                var restoreTime = LocalTime.Now;
                using (var connection = new SqlConnection(GetMasterConnectionString()))
                {
                    connection.Open();
                    var detachTime = LocalTime.Now;
                    await DetachDatabase(connection);

                    Debug.WriteLine("Total time for detaching database: " + LocalTime.Now.Subtract(detachTime).Milliseconds);

                    FileInfo mdfFile = null, ldfFile = null;

                    var copyTime = LocalTime.Now;
                    // copy each database file to its old place
                    foreach (var originFile in SnapshotsDirectory.GetFiles("*.origin"))
                    {
                        originFile.IsReadOnly = true;

                        var destination = await File.ReadAllTextAsync(originFile.FullName);
                        var source = originFile.FullName.TrimEnd(originFile.Extension).AsFile();

                        if (IsInShareSnapshotMode)
                        {
                            destination = destination.Replace(GetSnapshotFileName(originFile), DatabaseName);
                        }

                        if (destination.ToLower().EndsWith(".mdf"))
                            mdfFile = destination.AsFile();

                        if (destination.ToLower().EndsWith(".ldf"))
                            ldfFile = destination.AsFile();

                        await source.CopyToAsync(destination.AsFile(), overwrite: true);
                        // shall we backup the existing one and in case of any error restore it?
                    }

                    Debug.WriteLine("Total time for copying database: " + LocalTime.Now.Subtract(copyTime).Milliseconds);

                    if (mdfFile == null)
                        throw new Exception("Cannot find any MDF file in snapshot directory " + SnapshotsDirectory.FullName);

                    if (ldfFile == null)
                        throw new Exception("Cannot find any LDF file in snapshot directory " + SnapshotsDirectory.FullName);
                    var attachTime = LocalTime.Now;
                    await AttachDatabase(connection, mdfFile, ldfFile);
                    Debug.WriteLine("Total time for attaching database: " + LocalTime.Now.Subtract(attachTime).Milliseconds);
                    await Database.Instance.Refresh();
                }

                Debug.WriteLine("Total time for restoreing database: " + LocalTime.Now.Subtract(restoreTime).Milliseconds);
            }
            finally
            {
                if (lockTaken == true)
                {
                    SnapshotRestoreLock.ReleaseMutex();
                }
            }
        }

        async Task DetachDatabase(SqlConnection connection)
        {
            SqlConnection.ClearAllPools();

            using (var cmd = new SqlCommand(
                "USE Master; ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ALTER DATABASE [{0}] SET MULTI_USER; exec sp_detach_db '{0}'"
                .FormatWith(DatabaseName), connection))
                await cmd.ExecuteNonQueryAsync();
        }

        async Task AttachDatabase(SqlConnection connection, FileInfo mdfFile, FileInfo ldfFile)
        {
            using (var cmd = new SqlCommand(
                "USE Master; CREATE DATABASE [{0}] ON (FILENAME = '{1}'), (FILENAME = '{2}') FOR ATTACH"
                .FormatWith(DatabaseName, mdfFile.FullName, ldfFile.FullName), connection))
                await cmd.ExecuteNonQueryAsync();
        }

        async Task TakeDatabaseOffline(SqlConnection connection)
        {
            SqlConnection.ClearAllPools();

            using (var cmd = new SqlCommand(
                "USE Master; ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE;"
                .FormatWith(DatabaseName), connection))
                await cmd.ExecuteNonQueryAsync();
        }

        async Task TakeDatabaseOnline(SqlConnection connection)
        {
            using (var cmd = new SqlCommand(
                "USE Master; ALTER DATABASE [{0}] SET ONLINE;"
                .FormatWith(DatabaseName), connection))
                await cmd.ExecuteNonQueryAsync();
        }

        async Task<FileInfo[]> GetPhysicalFiles(SqlConnection connection)
        {
            var files = new List<FileInfo>();

            using (var cmd = new SqlCommand(
                "USE Master; SELECT physical_name FROM sys.master_files where database_id = DB_ID('{0}')"
                .FormatWith(DatabaseName), connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                    files.Add(Convert.ToString(reader[0]).AsFile());
            }

            if (files.Count == 0)
                throw new Exception("Cannot find physical file name for database: " + DatabaseName);

            return files.ToArray();
        }

        #endregion

        void SetupDirecory()
        {
            // make sure it is empty
            if (SnapshotsDirectory.Exists())
            {
                SnapshotsDirectory.Delete(recursive: true);
            }

            SnapshotsDirectory.Create();
        }

        /// <summary>
        /// Gets the list of current snapshots on disk.
        /// </summary>
        public static List<string> GetList(bool isSharedSnapshotMode)
        {
            if (!GetSnapshotsRoot(isSharedSnapshotMode).Exists()) return null;

            return GetSnapshotsRoot(isSharedSnapshotMode).GetDirectories().Select(f => f.Name.Substring(0, f.Name.LastIndexOf('_'))).ToList();
        }

        static DirectoryInfo GetSnapshotsRoot(bool isSharedSnapshotMode)
        {
            if (isSharedSnapshotMode)
            {
                return TestDatabaseGenerator.DatabaseStoragePath
                    .GetSubDirectory(DatabaseName.Split('.').First() + " SNAPSHOTS");
            }
            else
            {
                return TestDatabaseGenerator.DatabaseStoragePath.GetSubDirectory(DatabaseName + "\\SNAPSHOTS");
            }
        }

        static string GetMasterConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(Config.GetConnectionString("AppDatabase"))
            {
                InitialCatalog = "master"
            };

            return builder.ToString();
        }

        static string GetDatabaseName()
        {
            return new SqlConnectionStringBuilder(Config.GetConnectionString("AppDatabase"))
                .InitialCatalog
                .Or("")
                .TrimStart("[")
                .TrimEnd("]");
        }

        static string CreateSnapshotName(string name)
        {
            var schemaHash = new TestDatabaseGenerator(false, false).GetCurrentDatabaseCreationHash();
            return "{0}_{1}".FormatWith(name, schemaHash).Except(Path.GetInvalidFileNameChars()).ToString("");
        }

        enum CopyProcess { Backup, Restore }
    }
}