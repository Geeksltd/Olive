namespace Olive.Aws.S3Backup.SqlServer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Olive.Aws.S3Backup.SqlServer.Services;

    public static class StartupExtensions
    {
        public static IServiceCollection AddS3BackupSqlServer(this IServiceCollection services)
        {
            services.AddScoped<IS3BackupService, S3BackupService>();
            return services;
        }

        public static IApplicationBuilder UseS3BackupSqlServer(this IApplicationBuilder app)
        {
            app.Map("/olive/s3backup", builder =>
            {
                builder.Run(async x =>
                {
                    var backupService = x.RequestServices.GetRequiredService<IS3BackupService>();
                    await backupService.StartNewBackup();
                });
            });

            app.Map("/olive/s3backup-status", builder =>
            {
                builder.Run(async x =>
                {
                    var backupService = x.RequestServices.GetRequiredService<IS3BackupService>();
                    await backupService.GetBackupStatus();
                });
            });

            //app.Map("/olive/s3restore", builder =>
            //{
            //    builder.Run(async x =>
            //    {
            //        var backupService = x.RequestServices.GetRequiredService<IS3BackupService>();
            //        await backupService.StartRestore();
            //    });
            //});

            //app.Map("/olive/s3restore-status", builder =>
            //{
            //    builder.Run(async x =>
            //    {
            //        var backupService = x.RequestServices.GetRequiredService<IS3BackupService>();
            //        await backupService.GetRestoreStatus();
            //    });
            //});

            return app;
        }
    }
}
