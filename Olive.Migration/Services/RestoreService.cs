namespace Olive.Migration.Services
{
    using System;
    using System.Threading.Tasks;
	using Olive.Entities;
	using Olive.Migration.Services.Contracts;

    public class RestoreService : IRestoreService
	{
		private readonly IPathService _pathService;

		public RestoreService(IPathService pathService)
		{
			_pathService = pathService;
		}

		private IDatabase Db => Context.Current.Database();

		public async Task<(bool success, string errorMessage)> Restore(string path)
		{
			var commandRestore = $"exec msdb.dbo.rds_restore_database @restore_db_name='{_pathService.GetDatabaseName()}', @s3_arn_to_restore_from='{path}';";

			var restoreDataTable = await Db.GetAccess().ExecuteQuery(commandRestore);
			var taskId = restoreDataTable.Rows[0]["task_id"];

			var commandStatus = "exec msdb.dbo.rds_task_status @task_id=" + taskId;
			var statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
			var status = statusDataTable.Rows[0]["lifecycle"].ToString();

			while (status != "SUCCESS")
			{
				await Task.Delay(5000);
				statusDataTable = await Db.GetAccess().ExecuteQuery(commandStatus);
				status = statusDataTable.Rows[0]["lifecycle"].ToString();
			}

			return (true, "");
		}
	}
}