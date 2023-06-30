namespace Olive.Migration.Services
{
	using System;
	using System.Threading.Tasks;
	using Olive.Entities;
	using Olive.Migration.Services.Contracts;

	public class BackupService : IBackupService
	{
		private readonly IPathService _pathService;

		public BackupService(IPathService pathService)
		{
			_pathService = pathService;
		}

		private IDatabase Db => Context.Current.Database();

		public async Task<(bool success, string path, string errorMessage)> Backup(string taskName, WhichBackup whichBackup)
		{
			var bucket = Config.Get<string>("DatabaseBackupBucket", "onsuttoninstitutelive-rdsbackups");
			var s3Path = $"arn:aws:s3:::{bucket}/{_pathService.GetDatabaseName()}/{_pathService.GetBackupFileName(taskName, whichBackup)}";
			var commandBackup = $"exec msdb.dbo.rds_backup_database @source_db_name='{_pathService.GetDatabaseName()}', @s3_arn_to_backup_to='{s3Path}', @overwrite_S3_backup_file=1;";

			var backupDataTable = await Db.GetAccess().ExecuteQuery(commandBackup);
			var taskId = backupDataTable.Rows[0]["task_id"];

			var commandStatus = "exec msdb.dbo.rds_task_status @task_id=" + taskId;
			var statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
			var status = statusDataTable.Rows[0]["lifecycle"].ToString();

			while (status != "SUCCESS")
			{
				await Task.Delay(5000);
				statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
				status = statusDataTable.Rows[0]["lifecycle"].ToString();
			}

			return (true, s3Path, "");
		}
	}
}