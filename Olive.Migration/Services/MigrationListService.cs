namespace Olive.Migration.Services
{
    using Microsoft.AspNetCore.Http;
    using Olive.Entities;
    using Olive.Migration.Services.Contracts;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MigrationListService : IMigrationListService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IPathService _pathService;

		private IDatabase Db => Context.Current.Database();

		public MigrationListService(IHttpContextAccessor httpContextAccessor, IPathService pathService)
		{
			_httpContextAccessor = httpContextAccessor;
			_pathService = pathService;
		}

		public async Task<(List<IMigrationTask> tasks, List<string> errorMessages)> Get()
		{
			var list = new List<IMigrationTask>();
			var errors = new List<string>();

			var oldMigrations = await Db.GetList<IMigrationTask>();
			var newMigrations = new List<IMigrationTask>();

			var files = _pathService.MigrationFiles();

			foreach (var file in files)
			{
				if (!IsValidFileName(file.NameWithoutExtension()))
				{
					errors.Add(GetFileNameErrorMessage(file.NameWithoutExtension()));
					continue;
				}
				var oldMigration = oldMigrations.SingleOrDefault(a => a.Name == file.NameWithoutExtension());

				if (oldMigration != null)
				{
					list.Add(oldMigration);
				}
				else
				{
					var record = _httpContextAccessor.HttpContext.RequestServices.GetService(typeof(IMigrationTask)) as IMigrationTask;

					record.CreateOn = file.CreationTime;
					record.LoadOn = LocalTime.Now;
					record.Name = file.NameWithoutExtension();
					record.AfterMigrationBackupPath = "";
					record.BeforeMigrationBackupPath = "";
					record.Migrated = false;

					newMigrations.Add(record);
					list.Add(record);
				}
			}

			if (newMigrations.Any())
			{
				await Db.Save(newMigrations);
			}

			return (list, errors);
		}

		private bool IsValidFileName(string fileName)
		{
			if (fileName.IsEmpty() || fileName.Length < 12) return false;
			var numbers = fileName[..12];
			return long.TryParse(numbers, out var number) && number >= 0;
		}

		private string GetFileNameErrorMessage(string fileName)
		{
			return $"File name '{fileName}' is not valid";
		}
	}
}
