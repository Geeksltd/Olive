using Hangfire;
using Hangfire.MySql.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities.Data;
using System;
using System.Data;

namespace Olive.Hangfire.MySql
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
                c.UseStorage(new MySqlStorage(
                        Context.Current.GetService<IConnectionStringProvider>().GetConnectionString(),
                        new MySqlStorageOptions { TablePrefix = "Hangfire" })
                    );
                config?.Invoke(c);
            });

            Hangfire.ExtensionMethods.AddDevCommand(@this);

            return @this;
        }

        /// <summary>
        /// It will register the hangfire server.
        /// If a debugger is attached, it will also start the hangfire dashboard.
        /// </summary>      
        public static IApplicationBuilder UseScheduledTasks<TPlan>(this IApplicationBuilder @this)
            where TPlan : BackgroundJobsPlan, new()
        {
            return Hangfire.ExtensionMethods.UseScheduledTasks<TPlan>(@this);
        }
    }
}