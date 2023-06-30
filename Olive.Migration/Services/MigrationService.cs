namespace Olive.Migration.Services
{
	using Olive.Entities;
	using Olive.Migration.Services.Contracts;
	using Olive.Mvc;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Text;
	using System.Threading.Tasks;

	public class MigrationService : IMigrationService
	{
		private readonly IMigrationListService _migrationListService;
		private readonly IBackupService _backupService;
		private readonly IRestoreService _restoreService;
		private readonly IPathService _pathService;

		private IDatabase Db => Context.Current.Database();

		public MigrationService(IMigrationListService migrationListService, IBackupService backupService, IRestoreService restoreService, IPathService pathService)
		{
			_migrationListService = migrationListService;
			_backupService = backupService;
			_restoreService = restoreService;
			_pathService = pathService;
		}

		public async Task<(IMigrationTask task, string errorMessage)> Migrate(IMigrationTask task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			var (can, error) = await CanApply(task);
			if (!can)
			{
				await Db.Update(task, item =>
				{
					item.LastError = error;
				});
				return (task, error);
			}

			var commands = GetCommands(task);

			if (commands.None())
			{
				await Db.Update(task, item =>
				{
					item.LastError = "Migration file is empty";
				});
				return (task, "Migration file is empty");
			}

			var (success, path, errorMessage) = await _backupService.Backup(task.Name, WhichBackup.Before);

			if (!success)
			{
				await Db.Update(task, item =>
				{
					item.LastError = errorMessage;
				});
				return (task, errorMessage);
			}

			await Db.Update(task, item =>
			{
				item.BeforeMigrationBackupPath = path;
				item.BeforeMigrationBackupOn = LocalTime.Now;
				item.MigrateStartOn = LocalTime.Now;
			});
			task = await Db.Reload(task);

			(success, errorMessage) = await MigrateInternal(commands);

			if (!success)
			{
				await Db.Update(task, item =>
				{
					item.LastError = errorMessage;
				});
				return (task, errorMessage);
			}

			await Db.Update(task, item =>
			{
				item.Migrated = true;
				item.MigrateEndOn = LocalTime.Now;
			});
			task = await Db.Reload(task);

			(success, path, errorMessage) = await _backupService.Backup(task.Name, WhichBackup.After);

			if (!success)
			{
				await Db.Update(task, item =>
				{
					item.LastError = errorMessage;
				});
				return (task, errorMessage);
			}

			await Db.Update(task, item =>
			{
				item.AfterMigrationBackupPath = path;
				item.AfterMigrationBackupOn = LocalTime.Now;
				item.LastError = "";
			});

			return (task, "");
		}

		private string[] GetCommands(IMigrationTask task)
		{

			var file = _pathService.MigrationFile(task.Name);
			var sql = File.ReadAllLines(file.FullName);
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
			return commands.Where(a => !a.Trim().IsEmpty()).ToArray();
		}

		private async Task<(bool success, string errorMessage)> MigrateInternal(string[] commands)
		{
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
			if (task.Migrated)
			{
				return (false, "Migration already done.");
			}

			var files = _pathService.MigrationFiles();
			if (files.None()) return (false, "Migration file not exists");

			var allDbMigrations = await _migrationListService.GetDatabaseList();

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

		public Task<(IMigrationTask task, string errorMessage)> Restore(IMigrationTask task, WhichBackup whichBackup)
		{
			return whichBackup == WhichBackup.Before ? RestoreBefore(task) : RestoreAfter(task);
		}

		private async Task<(IMigrationTask task, string errorMessage)> RestoreBefore(IMigrationTask task)
		{
			if (task.BeforeMigrationBackupPath.IsEmpty())
			{
				await Db.Update(task, item =>
				{
					item.LastError = "Before migration backup path is empty";
				});
				return (task, "Before migration backup path is empty");
			}

			var (success, errorMessage) = await _restoreService.Restore(task.BeforeMigrationBackupPath);

			if (!success)
			{
				await Db.Update(task, item =>
				{
					item.LastError = errorMessage;
				});
				return (task, errorMessage);
			}

			await Db.Update(task, item =>
			{
				item.BeforeMigrationRestoreOn = LocalTime.Now;
			});

			return (task, "");
		}

		private async Task<(IMigrationTask task, string errorMessage)> RestoreAfter(IMigrationTask task)
		{
			if (task.AfterMigrationBackupPath.IsEmpty())
			{
				await Db.Update(task, item =>
				{
					item.LastError = "After migration backup path is empty";
				});
				return (task, "After migration backup path is empty");
			}

			var (success, errorMessage) = await _restoreService.Restore(task.AfterMigrationBackupPath);

			if (!success)
			{
				await Db.Update(task, item =>
				{
					item.LastError = errorMessage;
				});
				return (task, errorMessage);
			}

			await Db.Update(task, item =>
			{
				item.AfterMigrationRestoreOn = LocalTime.Now;
			});

			return (task, "");
		}
	}
}