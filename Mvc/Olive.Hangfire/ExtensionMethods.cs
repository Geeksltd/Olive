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
            void start()
            {
                @this.UseHangfireServer();

                if (System.Diagnostics.Debugger.IsAttached)
                    @this.UseHangfireDashboard();

                createAutomatedTasks();
            }

            if (Context.Current.Environment().IsProduction()) start();
            else Context.StartedUp.Handle(() => start());

            return @this;
        }
    }
}