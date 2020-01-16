using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities.Data;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Olive.Hangfire.MySql")]

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
                c.UseSqlServerStorage(Context.Current.GetService<IConnectionStringProvider>()
                    .GetConnectionString("Default"));
                config?.Invoke(c);
            });

            @this.AddDevCommand();

            return @this;
        }

        internal static void AddDevCommand(this IServiceCollection @this) =>
            @this.AddSingleton<IDevCommand, ShceduledTasksDevCommand>();

        /// <summary>
        /// It will register the hangfire server.
        /// If a debugger is attached, it will also start the hangfire dashboard.
        /// </summary>      
        public static IApplicationBuilder UseScheduledTasks<TPlan>(this IApplicationBuilder @this)
            where TPlan : BackgroundJobsPlan, new()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                @this.UseHangfireDashboard();

            @this.UseHangfireServer();

            var plan = new TPlan();
            plan.Initialize();

            if (Config.Get<bool>("Automated.Tasks:Enabled"))
            {
                foreach (var job in BackgroundJobsPlan.Jobs.Values)
                    RecurringJob.AddOrUpdate(job.Name, job.Action, job.ScheduleCron, queue: job.SyncGroup);
            }

            return @this;
        }

    }
}