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

		public async Task<IMigrationTask[]> GetDatabaseList()
		{
			try
			{
				return await Db.GetList<IMigrationTask>();
			}
			catch (System.Exception e)
			{
				if(e.InnerException?.Message == "Invalid object name 'MigrationTasks'.")
				{
					await CreateTable();
				}
				return new IMigrationTask[0];
			}
		}

		public async Task<(List<IMigrationTask> tasks, List<string> errorMessages)> GetFilesList()
		{
			var list = new List<IMigrationTask>();
			var errors = new List<string>();

			var oldMigrations = await GetDatabaseList();
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

		private async Task CreateTable()
		{
			await Db.GetAccess().ExecuteNonQuery(@"
CREATE TABLE [dbo].[MigrationTasks](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[CreateOn] [datetime2](7) NOT NULL,
	[LoadOn] [datetime2](7) NOT NULL,
	[MigrateStartOn] [datetime2](7) NULL,
	[MigrateEndOn] [datetime2](7) NULL,
	[Migrated] [bit] NOT NULL,
	[BeforeMigrationBackupPath] [nvarchar](200) NULL,
	[BeforeMigrationBackupOn] [datetime2](7) NULL,
	[BeforeMigrationRestoreOn] [datetime2](7) NULL,
	[AfterMigrationBackupPath] [nvarchar](200) NULL,
	[AfterMigrationBackupOn] [datetime2](7) NULL,
	[AfterMigrationRestoreOn] [datetime2](7) NULL,
	[LastError] [nvarchar](max) NULL,
PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
");
		}
	}
}
