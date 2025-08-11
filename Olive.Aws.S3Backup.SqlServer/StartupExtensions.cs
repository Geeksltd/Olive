namespace Olive.Aws.S3Backup.SqlServer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Olive.Aws.S3Backup.SqlServer.Services;

    public static class StartupExtensions
    {
        public static void AddS3BackupSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IS3BackupService, S3BackupService>();
        }

        public static void UseS3BackupSqlServer(this IApplicationBuilder app)
        {
            app.Map("/olive/s3backup", async (context) =>
            {
                var backupService = context.ApplicationServices.GetRequiredService<IS3BackupService>();
                await backupService.StartNewBackup();
            });

            app.Map("/olive/s3backup-status", async (context) =>
            {
                var backupService = context.ApplicationServices.GetRequiredService<IS3BackupService>();
                await backupService.GetBackupStatus();
            });

            //app.Map("/olive/s3restore", async (context) =>
            //{
            //    var backupService = context.ApplicationServices.GetRequiredService<IS3BackupService>();
            //    await backupService.StartRestore();
            //});

            //app.Map("/olive/s3restore-status", async (context) =>
            //{
            //    var backupService = context.ApplicationServices.GetRequiredService<IS3BackupService>();
            //    await backupService.GetRestoreStatus();
            //});
        }
    }
}
