namespace Olive.Aws.S3Backup.SqlServer.Services
{
    using Microsoft.AspNetCore.Http;
    using Olive.BlobAws;
    using Olive.Entities;
    using System.Threading.Tasks;

    public interface IS3BackupService
    {
        Task StartNewBackup();
        Task GetBackupStatus();
        //Task StartRestore();
        //Task GetRestoreStatus();
    }

    public class S3BackupService(IHttpContextAccessor httpContextAccessor) : IS3BackupService
    {
        IDatabase Db => Context.Current.Database();
        bool IsInDevRole => httpContextAccessor.HttpContext?.User.IsInRole("Dev") ?? false;
        bool IsInDevOpsRole => httpContextAccessor.HttpContext?.User.IsInRole("DevOps") ?? false;
        string GetDatabaseName()
        {
            var connectionString = Config.Get<string>("ConnectionStrings:Default");
            System.Data.Common.DbConnectionStringBuilder builder = new System.Data.Common.DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            return builder["Database"] as string ?? throw new Exception("Unable to determine the database name.");
        }

        public string GetBackupFileName()
        {
            var now = LocalTime.Now;
            return $"{now.Year:D4}-{now.Month:D2}-{now.Day:D2}-{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}";
        }

        public async Task StartNewBackup()
        {
            if (!IsInDevRole)
                throw new UnauthorizedAccessException("You do not have permission to start new backup.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var database = GetDatabaseName();
            var backup = GetBackupFileName();
            var s3BackupPath = $"arn:aws:s3:::{bucket}/{database}/{backup}.bak";
            var s3InfoPath = $"{database}/{backup}.json";
            var commandBackup = $"exec msdb.dbo.rds_backup_database @source_db_name='{database}', @s3_arn_to_backup_to='{s3BackupPath}', @overwrite_S3_backup_file=1;";

            var backupDataTable = await Db.GetAccess().ExecuteQuery(commandBackup);
            var taskId = backupDataTable.Rows[0]["task_id"] as string;

            await RawS3.WriteTextFile(s3InfoPath, taskId);

            httpContextAccessor.HttpContext?.Response.Redirect($"/olive/s3backup-status?backup={backup}");
        }

        public async Task GetBackupStatus()
        {
            if (httpContextAccessor.HttpContext is null || !IsInDevRole)
                throw new UnauthorizedAccessException("You do not have permission to access backup status.");

            var bucket = Config.GetOrThrow("Blob:S3:BackupBucket");
            var backup = httpContextAccessor.HttpContext?.Request.Query["backup"].ToString() ?? throw new ArgumentException("Backup name is required in the query string.");
            var database = GetDatabaseName();
            var taskId = await RawS3.ReadTextFile(bucket, $"{database}/{backup}.json");

            var commandStatus = "exec msdb.dbo.rds_task_status @task_id=" + taskId;
            var statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
            var status = statusDataTable.Rows[0]["lifecycle"].ToString();

            httpContextAccessor.HttpContext.Response.ContentType = "text/plain";
            httpContextAccessor.HttpContext.Response.StatusCode = 200;
            await httpContextAccessor.HttpContext.Response.WriteAsync($"Status for backup {backup}.bak is '{status}'");
        }

        //public Task StartRestore()
        //{
        //    if (!IsInDevOpsRole)
        //        throw new UnauthorizedAccessException("You do not have permission to restore database.");
        //}

        //public Task<string> GetRestoreStatus()
        //{
        //    if (!IsInDevOpsRole)
        //        throw new UnauthorizedAccessException("You do not have permission to access restore status.");
        //}
    }
}
