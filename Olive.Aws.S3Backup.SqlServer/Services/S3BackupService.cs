namespace Olive.Aws.S3Backup.SqlServer.Services
{
    using Microsoft.AspNetCore.Http;
    using Olive.BlobAws;
    using Olive.Entities;
    using Olive.Entities.Data;
    using System.IO;
    using System.Threading.Tasks;

    public interface IS3BackupService
    {
        Task GetBackupFiles();
        Task StartNewBackup();
        Task GetBackupStatus();
        Task StartRestore();
        Task GetRestoreStatus();
    }

    public class S3BackupService(IHttpContextAccessor httpContextAccessor) : IS3BackupService
    {
        IDatabase Db => Context.Current.Database();
        bool IsInDevRole => httpContextAccessor.HttpContext?.User.IsInRole("Dev") ?? false;
        bool IsInDevOpsRole => httpContextAccessor.HttpContext?.User.IsInRole("DevOps") ?? false;
        bool IsInDevOpsOrDevRole => IsInDevOpsRole || IsInDevRole;
        string GetDatabaseName()
        {
            var connectionString = Config.Get<string>("ConnectionStrings:Default");
            System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            return builder["Database"] as string ?? throw new Exception("Unable to determine the database name.");
        }

        string GetBackupFileName()
        {
            var now = LocalTime.Now;
            return $"{now.Year:D4}-{now.Month:D2}-{now.Day:D2}-{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}";
        }

        string GetConnectionString(string database)
        {
            var connectionString = Config.Get<string>("ConnectionStrings:BackupRestore");
            System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            builder["Database"] = database;
            return builder.ConnectionString;
        }

        public async Task GetBackupFiles()
        {
            if (httpContextAccessor.HttpContext is null || !IsInDevOpsRole)
                throw new UnauthorizedAccessException("You do not have permission to see the list of backup files.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var database = GetDatabaseName();
            var files = await RawS3.GetFiles(bucket, $"{database}/", ".bak");

            httpContextAccessor.HttpContext.Response.ContentType = "text/html";

            if (files.Count == 0)
            {
                httpContextAccessor.HttpContext.Response.StatusCode = 404;
                await httpContextAccessor.HttpContext.Response.WriteAsync("No backup files found.");
                return;
            }

            httpContextAccessor.HttpContext.Response.StatusCode = 200;
            await httpContextAccessor.HttpContext.Response.WriteAsync(files.ToString("<br/>"));
        }

        public async Task StartNewBackup()
        {
            if (!IsInDevOpsOrDevRole)
                throw new UnauthorizedAccessException("You do not have permission to start new backup.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var database = GetDatabaseName();
            var backup = GetBackupFileName();
            var s3BackupPath = $"arn:aws:s3:::{bucket}/{database}/{backup}.bak";
            var s3InfoPath = $"{database}/{backup}.json";

            var commandBackup = $"exec msdb.dbo.rds_backup_database @source_db_name='{database}', @s3_arn_to_backup_to='{s3BackupPath}', @overwrite_S3_backup_file=1;";

            using var dc = new DatabaseContext(GetConnectionString(database));
            var backupDataTable = await Db.GetAccess().ExecuteQuery(commandBackup);
            var taskId = backupDataTable.Rows[0]["task_id"] as string;

            await RawS3.WriteJsonFile(s3InfoPath, new InfoData { Database = database, BackupTask = taskId });

            httpContextAccessor.HttpContext?.Response.Redirect($"/olive/s3backup-status?backup={backup}");
        }

        public async Task GetBackupStatus()
        {
            if (httpContextAccessor.HttpContext is null || !IsInDevOpsOrDevRole)
                throw new UnauthorizedAccessException("You do not have permission to access backup status.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var backup = httpContextAccessor.HttpContext?.Request.Query["backup"].ToString() ?? throw new ArgumentException("Backup name is required in the query string.");
            var database = GetDatabaseName();
            var s3InfoPath = $"{database}/{backup}.json";
            var info = await RawS3.ReadJsonFile<InfoData>(bucket, s3InfoPath);
            if (info.Database != database)
                throw new Exception($"Backup '{backup}' does not belong to the current database '{database}'.");
            var taskId = info.BackupTask ?? throw new Exception("Invalid backup task id");

            var commandStatus = "exec msdb.dbo.rds_task_status @task_id=" + taskId;
            using var dc = new DatabaseContext(GetConnectionString(database));
            var statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
            var status = statusDataTable.Rows[0]["lifecycle"].ToString();

            var message = $"Status for backup {backup}.bak is '{status}'";

            if ("SUCCESS".Equals(status, StringComparison.OrdinalIgnoreCase))
            {
                var preSignedURL = await RawS3.GeneratePreSignedURL(bucket, $"{database}/{backup}.bak", TimeSpan.FromMinutes(30));
                message += $" and the backup file is available <a href='{preSignedURL}'>HERE</a>";
            }

            httpContextAccessor.HttpContext.Response.ContentType = "text/html";
            httpContextAccessor.HttpContext.Response.StatusCode = 200;
            await httpContextAccessor.HttpContext.Response.WriteAsync(message);
        }

        public async Task StartRestore()
        {
            if (httpContextAccessor.HttpContext is null || !IsInDevOpsRole)
                throw new UnauthorizedAccessException("You do not have permission to restore database.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var backup = httpContextAccessor.HttpContext?.Request.Query["backup"].ToString() ?? throw new ArgumentException("Backup name is required in the query string.");
            var database = GetDatabaseName();

            var s3InfoPath = $"{database}/{backup}.json";
            var info = await RawS3.ReadJsonFile<InfoData>(bucket, s3InfoPath);
            if (info.Database != database)
                throw new Exception($"Backup '{backup}' does not belong to the current database '{database}'.");

            var s3BackupPath = $"{database}/{backup}.bak";
            if (!await RawS3.Exists(bucket, s3BackupPath))
                throw new FileNotFoundException($"Backup file '{s3BackupPath}' does not exist in S3 bucket '{bucket}'.");
            var s3BackupArn = $"arn:aws:s3:::{bucket}/{s3BackupPath}";

            var commandRestore = $"exec msdb.dbo.rds_restore_database @restore_db_name='{database}', @s3_arn_to_restore_from='{s3BackupArn}';";
            using var dc = new DatabaseContext(GetConnectionString(database));
            var restoreDataTable = await Db.GetAccess().ExecuteQuery(commandRestore);
            var taskId = restoreDataTable.Rows[0]["task_id"];
            info.RestoreTask = taskId as string;
            await RawS3.WriteJsonFile(s3InfoPath, info);

            httpContextAccessor.HttpContext?.Response.Redirect($"/olive/s3restore-status?backup={backup}");
        }

        public async Task GetRestoreStatus()
        {
            if (httpContextAccessor.HttpContext is null || !IsInDevOpsRole)
                throw new UnauthorizedAccessException("You do not have permission to access restore status.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var backup = httpContextAccessor.HttpContext?.Request.Query["backup"].ToString() ?? throw new ArgumentException("Backup name is required in the query string.");
            var database = GetDatabaseName();

            var s3InfoPath = $"{database}/{backup}.json";
            var info = await RawS3.ReadJsonFile<InfoData>(bucket, s3InfoPath);
            if (info.Database != database)
                throw new Exception($"Backup '{backup}' does not belong to the current database '{database}'.");
            var taskId = info.RestoreTask ?? throw new Exception("Invalid backup task id");

            var commandStatus = "exec msdb.dbo.rds_task_status @task_id=" + taskId;
            using var dc = new DatabaseContext(GetConnectionString(database));
            var statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
            var status = statusDataTable.Rows[0]["lifecycle"].ToString();

            var message = $"Status for restoration of {backup}.bak is '{status}'";

            httpContextAccessor.HttpContext.Response.ContentType = "text/html";
            httpContextAccessor.HttpContext.Response.StatusCode = 200;
            await httpContextAccessor.HttpContext.Response.WriteAsync(message);
        }

        public class InfoData
        {
            public string? BackupTask { get; set; }
            public string? RestoreTask { get; set; }
            public required string Database { get; set; }
        }
    }
}
