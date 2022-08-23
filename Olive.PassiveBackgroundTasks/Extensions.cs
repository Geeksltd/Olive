using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive.PassiveBackgroundTasks
{
    public static class Extensions
    {
        public static IServiceCollection AddScheduledTasks<TPlan, TBackgroundTask>(this IServiceCollection services)
            where TBackgroundTask : class, IBackgourndTask
            where TPlan : BackgroundJobsPlan, new()
        {
            services.AddSingleton<IBackgourndTask, TBackgroundTask>();
            services.AddSingleton<BackgroundJobsPlan, TPlan>();
            return services;
        }

        public static async Task<IApplicationBuilder> UseScheduledTasks<TPlan>(this IApplicationBuilder app, string pathMatch = "/olive-trigger-tasks")
            where TPlan : BackgroundJobsPlan, new()
        {
            if (!Config.Get<bool>("Automated.Tasks:Enabled")) return app;

            var plan = Context.Current.GetService<BackgroundJobsPlan>();
            plan.Initialize();

            foreach (var job in BackgroundJobsPlan.Jobs.Values)
            {
                Log.For<IBackgourndTask>()
                    .Info("Registering " + job.Name + " cron : " + job.ScheduleCron + " -> " + CronParser.Minutes(job.ScheduleCron) + " minutes");
                await Engine.Register(job.Name, job.Action, CronParser.Minutes(job.ScheduleCron), job.TimeoutInMinutes).ConfigureAwait(false);
            }

            await Engine.Cleanup(BackgroundJobsPlan.Jobs.Values.Select(a => a.Name).ToArray());

            app.Map(pathMatch, x => x.UseMiddleware<DistributedBackgroundTasksMiddleware>());

            return app;
        }
    }
}