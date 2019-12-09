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

namespace Olive.DistributedBackgroundTasks
{
    public static class Extensions
    {
        static IDatabase Database => Context.Current.Database();
        public static IServiceCollection AddDistributedBackgroundTasks<T>(this IServiceCollection services) where T : class, IBackgourndTask
        {
            services.AddSingleton<IBackgourndTask, T>();
            return services;
        }

        public static IApplicationBuilder UseDistributedBackgroundTasks(this IApplicationBuilder app, Action<BackgroundProcessManager> manager, string pathMatch = "/trigger")
        {
            manager(BackgroundProcessManager.Current);
            app.Map(pathMatch, x => x.UseMiddleware<DistributedBackgroundTasksMiddleware>());
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

        internal static Task<IBackgourndTask> SendHeartbeat(this IBackgourndTask task)
        {
            task.Logger().Info("Recording heartbeat for " + task.Name);

            return Update(task, t =>
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

        internal static async Task<bool> TryPick(this IBackgourndTask task)
        {
            Console.WriteLine($"Checking to see if {task.Name} has to run");
            using (var scope = Database.CreateTransactionScope())
            {
                // cause a distributed lock
                await DataAccess.Create().ExecuteNonQuery($"update {task.GetType().Name.ToPlural()} set Heartbeat = Heartbeat");

                task = await Database.Reload(task);

                var lastExecuted = task.LastExecuted.GetValueOrDefault();
                var nextExecution = lastExecuted.AddMinutes(task.IntervalInMinutes);

                if (nextExecution.IsInTheFuture())
                {
                    task.Logger().Info($"Should still wait for running [{task.Name}]. Next execution is at {nextExecution}.");
                    return false;
                }

                var lastHeartbeat = task.Heartbeat.GetValueOrDefault();
                var stillAlive = lastHeartbeat.AddMinutes(task.TimeoutInMinutes).IsInTheFuture();

                if (stillAlive)
                {
                    task.Logger().Info($"[{task.Name}] is already running on [{task.ExecutingInstance}] instance.");
                }
                else
                {
                    task.Logger().Info($"[{task.Name}] is not running. Last heartbeat : " + lastHeartbeat);
                    await task.SendHeartbeat();
                }

                scope.Complete();

                return !stillAlive;
            }
        }

        internal static ILogger Logger(this object @this) => Log.For(@this);
    }
}
