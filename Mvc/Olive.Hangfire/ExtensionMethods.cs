using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Olive.Mvc;

namespace Olive.Hangfire
{
    public static class ExtensionMethods
    {
        public static IDevCommandsConfig AddTasks(this IDevCommandsConfig @this)
        {
            @this.Add("tasks", async () =>
            {
                await Context.Current.Response().EndWith(@"
         <html>
            <body>
               <script src='https://ajax.googleapis.com/ajax/libs/jquery/2.1.3/ jquery.min.js'>
               </script>
               TODO: Add the hangfire tasks information here.
            </body>
         </html>");
                return true;
            }, "Tasks...");

            return @this;
        }

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

            return @this;
        }

        /// <summary>
        /// It will register the hangfire server.
        /// If a debugger is attached, it will also start the hangfire dashboard.
        /// </summary>      
        public static IApplicationBuilder UseScheduledTasks(this IApplicationBuilder @this,
            Action createAutomatedTasks)
        {
            Context.StartedUp.Handle(() =>
            {
                @this.UseHangfireServer();

                if (System.Diagnostics.Debugger.IsAttached)
                    @this.UseHangfireDashboard();

                createAutomatedTasks();
            });

            return @this;
        }
    }
}