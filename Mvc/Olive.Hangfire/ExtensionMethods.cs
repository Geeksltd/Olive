using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Olive.Mvc.Testing;
using Olive.Web;

namespace Olive.Hangfire
{
    public static class ExtensionMethods
    {
        public static IWebTestConfig AddTasks(this IWebTestConfig config)
        {
            config.Add("tasks", async () =>
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

            return config;
        }

        /// <summary>
        /// Adds the hangfire service and configure it to use Sql Server Storage with the connection string that of AppDatabase.
        /// </summary>      
        public static IServiceCollection AddScheduledTasks(this IServiceCollection services, Action<IGlobalConfiguration> config = null)
        {
            services.AddHangfire(c =>
            {
                c.UseSqlServerStorage(Config.GetConnectionString("AppDatabase"));
                config?.Invoke(c);
            });

            return services;
        }

        /// <summary>
        /// It will register the hangfire server.
        /// If a debugger is attached, it will also start the hangfire dashboard.
        /// </summary>      
        public static IApplicationBuilder UseScheduledTasks(this IApplicationBuilder app, Action createAutomatedTasks)
        {
            app.UseHangfireServer();

            if (System.Diagnostics.Debugger.IsAttached)
                app.UseHangfireDashboard();

            createAutomatedTasks();

            return app;
        }
    }
}
