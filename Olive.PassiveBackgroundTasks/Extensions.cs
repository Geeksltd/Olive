using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.PassiveBackgroundTasks
{
    public static class Extensions
    {
        static IDatabase Database => Context.Current.Database();
        public static IServiceCollection AddScheduledTasks<T>(this IServiceCollection services) where T : class, IBackgourndTask
        {
            services.AddSingleton<IBackgourndTask, T>();
            return services;
        }

        public static async Task<IApplicationBuilder> UseScheduledTasks<TPlan>(this IApplicationBuilder app, string pathMatch = "/olive-trigger-tasks")
            where TPlan : BackgroundJobsPlan, new()
        {
            if (Config.Get<bool>("Automated.Tasks:Enabled"))
            {
                var plan = new TPlan();
                plan.Initialize();

                foreach (var job in BackgroundJobsPlan.Jobs.Values)
                {
                    app.Logger().Info("Registering " + job.Name + " cron : " + job.ScheduleCron + " -> " + CronParser.Minutes(job.ScheduleCron) + " minutes");
                    await BackgroundProcessManager.Current.Register(job.Name, job.Action, CronParser.Minutes(job.ScheduleCron), job.TimeoutInMinutes);
                    app.Logger().Info("Registered " + job.Name);
                }

                app.Map(pathMatch, x => x.UseMiddleware<DistributedBackgroundTasksMiddleware>());
            }

            return app;
        }

        internal static Task GetActionTask(this IBackgourndTask task) =>
            BackgroundProcessManager.Current.GetAction(task);

        internal static Task RecordExecution(this IBackgourndTask task)
        {
            task.Logger().Info("Recording execution for " + task.Name);
            return Update(task, t =>
            {
                t.LastExecuted = LocalTime.Now;
                t.ExecutingInstance = null;
                t.Heartbeat = null;
            });
        }

        internal static async Task<IBackgourndTask> SendHeartbeat(this IBackgourndTask task)
        {
            task.Logger().Info("Recording heartbeat for " + task.Name + " from instance : " + ExecutionEngine.Id);

            return await Update(task, t =>
             {
                 t.Heartbeat = LocalTime.Now;
                 t.ExecutingInstance = ExecutionEngine.Id;
             });
        }

        static async Task<IBackgourndTask> Update(IBackgourndTask task, Action<IBackgourndTask> action)
        {
            var clone = (IBackgourndTask)(await Database.Reload(task)).Clone();

            action(clone);

            clone = await Database.Save(clone);

            return await Database.Reload(clone);
        }
        static void LogInfo(string message) => Log.For<IBackgourndTask>().Info(LocalTime.UtcNow.ToString("HH:mm:ss") + " : " + message);

        internal static async Task<bool> TryPick(this IBackgourndTask task)
        {
            var result = false;

            LogInfo($"Checking to see if {task.Name} has to run.");
            using (var scope = Database.CreateTransactionScope())
            {
                LogInfo($"Causing a distributed lock for {task.Name}.");
                // cause a distributed lock
                await DataAccess.Create().ExecuteNonQuery($"update {task.GetType().Name.ToPlural()} set Heartbeat = Heartbeat where Name = '{task.Name}'");

                task = await Database.Reload(task);

                var lastExecuted = task.LastExecuted.GetValueOrDefault();
                var nextExecution = lastExecuted.AddMinutes(task.IntervalInMinutes);

                if (nextExecution.IsInTheFuture())
                {
                    LogInfo($"Should still wait for running [{task.Name}]. Next execution is at {nextExecution}.");
                    result = false;
                }
                else
                {
                    LogInfo($"{task.Name} should run.");
                    var lastHeartbeat = task.Heartbeat.GetValueOrDefault();
                    var stillAlive = lastHeartbeat.AddMinutes(task.TimeoutInMinutes).IsInTheFuture();

                    if (stillAlive)
                    {
                        LogInfo($"[{task.Name}] is already running on [{task.ExecutingInstance}] instance.");
                    }
                    else
                    {
                        LogInfo($"[{task.Name}] is not running. Last heartbeat : " + lastHeartbeat);
                        await task.SendHeartbeat();
                    }

                    result = !stillAlive;
                }

                scope.Complete();

                return result;
            }
        }

        internal static ILogger Logger(this object @this) => Log.For(@this);
    }
}
