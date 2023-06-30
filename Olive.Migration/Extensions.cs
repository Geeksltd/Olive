namespace Olive.Migration
{
    using Microsoft.Extensions.DependencyInjection;
	using Olive.Migration.Services.Contracts;
	using Olive.Migration.Services;
	using Microsoft.AspNetCore.Mvc.ApplicationParts;

	public static class Extensions
	{
		public static IServiceCollection AddMigrations<TMigrationTask>(this IServiceCollection services)
			where TMigrationTask : class, IMigrationTask,new()
		{
			services.AddScoped<IMigrationListService, MigrationListService>();
			services.AddScoped<IMigrationService, MigrationService>();
			services.AddScoped<IBackupService, BackupService>();
			services.AddScoped<IRestoreService, RestoreService>();
			services.AddScoped<IPathService, PathService>();

			services.AddTransient<IMigrationTask, TMigrationTask>();

			services.AddControllers()
				.PartManager.ApplicationParts.Add(new AssemblyPart(typeof(Extensions).Assembly));

			return services;
		}

	}
}
