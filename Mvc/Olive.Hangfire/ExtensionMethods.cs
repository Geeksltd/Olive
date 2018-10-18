using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Olive.Hangfire
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Adds the hangfire service and configure it to use Sql Server Storage with the connection string that of AppDatabase.
        /// </summary>      
        public static IServiceCollection AddScheduledTasks(this IServiceCollection @this, Action<IGlobalConfiguration> config = null)
        {
            @this.AddHangfire(c =>
            {
                c.UseSqlServerStorage(Config.GetConnectionString("Default"));
                config?.Invoke(c);
            });

            @this.AddSingleton<IDevCommand, ShceduledTasksDevCommand>();

            return @this;
        }

        /// <summary>
        /// It will register the hangfire server.
        /// If a debugger is attached, it will also start the hangfire dashboard.
        /// </summary>      
        public static IApplicationBuilder UseScheduledTasks(this IApplicationBuilder @this,
            Action createAutomatedTasks)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                @this.UseHangfireDashboard();

            @this.UseHangfireServer();

            createAutomatedTasks();

            return @this;
        }
    }
}