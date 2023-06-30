namespace Olive.Migration.Services
{
	using Olive.Entities;
	using Olive.Migration.Services.Contracts;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class MigrationService : IMigrationService
	{
		private readonly IBackupService _backupService;
		private readonly IPathService _pathService;

		private IDatabase Db => Context.Current.Database();

		public MigrationService(IBackupService backupService, IPathService pathService)
		{
			_backupService = backupService;
			_pathService = pathService;
		}

		public async Task<(IMigrationTask task, string errorMessage)> Migrate(IMigrationTask task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			if (task.Migrated)
			{
				return (task, "Migration already done.");
			}

			var (success, path, errorMessage) = await _backupService.Backup();

			if (!success)
			{
				return (task, errorMessage);
			}

			task.BeforeMigrationBackupPath = path;
			await Context.Current.Database().Save(task);

			(success, errorMessage) = await MigrateInternal(task);

			if (!success)
			{
				return (task, errorMessage);
			}

			(success, path, errorMessage) = await _backupService.Backup();

			if (!success)
			{
				return (task, errorMessage);
			}

			task.AfterMigrationBackupPath = path;
			await Context.Current.Database().Save(task);

			return (task, "");
		}

		private async Task<(bool success, string errorMessage)> MigrateInternal(IMigrationTask task)
		{
			var (can, errorMessage) = await CanApply(task);
			if (!can)
			{
				return (false, errorMessage);
			}

			var file = _pathService.MigrationFile(task.Name);
			var sql = await File.ReadAllLinesAsync(file.FullName);
			var commands = new List<string>();
			var currentCommand = new StringBuilder();
			foreach (var cmd in sql)
			{
				if (cmd.ToLower().Trim() == "go")
				{
					commands.Add(currentCommand.ToString());
					currentCommand.Clear();
					continue;
				}
				currentCommand.AppendLine(cmd);
			}
			commands.Add(currentCommand.ToString());
			commands = commands.Where(a => !a.Trim().IsEmpty()).ToList();

			if (commands.None())
			{
				return (false, "Migration file is empty");
			}

			using (var transaction = Db.CreateTransactionScope())
			{
				try
				{
					foreach (var cmd in commands)
					{
						await Db.GetAccess().ExecuteNonQuery(cmd);
					}

					transaction.Complete();
					return (true, "");
				}
				catch (Exception e)
				{
					return (false, e.ToFullMessage());
				}
			}
		}

		private async Task<(bool can, string errorMessage)> CanApply(IMigrationTask task)
		{
			var files = _pathService.MigrationFiles();
			if (files.None()) return (false, "Migration file not exists");

			var allDbMigrations = await Db.GetList<IMigrationTask>();

			var notMigratedFiles = new List<string>();

			foreach (var item in files)
			{
				if (item.NameWithoutExtension() == task.Name) break;
				var dbItem = allDbMigrations.SingleOrDefault(a => a.Name == item.NameWithoutExtension());
				if (dbItem?.Migrated == true) continue;
				notMigratedFiles.Add(item.NameWithoutExtension());
			}

			if (notMigratedFiles.None()) return (true, "");

			return (false, $"Please migrate these files first: [{string.Join(", ", notMigratedFiles)}]");
		}
	}
}