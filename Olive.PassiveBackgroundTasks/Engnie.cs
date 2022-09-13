using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Olive.Entities;

namespace Olive.PassiveBackgroundTasks
{
    internal static class Engine
    {
        internal static Guid ExecutionId { get; } = Guid.NewGuid();

        static IDatabase Db => Context.Current.Database();

        static Dictionary<string, Func<Task>> Actions = new Dictionary<string, Func<Task>>();

        static ILogger Log => Olive.Log.For(typeof(Engine));

        internal static async Task Register(string name, Expression<Func<Task>> action, int intervalInMinutes, int timeoutInMinutes = 5)
        {
            var instance = await Db.FirstOrDefault<IBackgourndTask>(b => b.Name == name).ConfigureAwait(false);
            if (instance is null) instance = Context.Current.GetService<IBackgourndTask>();
            else instance = (IBackgourndTask)instance.Clone();

            instance.Name = name;
            instance.IntervalInMinutes = intervalInMinutes;
            instance.TimeoutInMinutes = timeoutInMinutes;

            Actions[name] = action.Compile();

            await Db.Save(instance).ConfigureAwait(false);

            Log.Info("Registered a background task for " + name);
        }

        internal static async Task CleanUp(params string[] validNames)
        {
            await Db.DeleteAll<IBackgourndTask>(a => a.Name.IsNoneOf(validNames));

            Log.Info("Clean up background tasks");
        }

        internal static Task GetAction(IBackgourndTask task) => Actions[task.Name]();

        static bool ShouldRun(IBackgourndTask task)
        {
            var nextExecution = task.LastExecuted.GetValueOrDefault().AddMinutes(task.IntervalInMinutes);

            if (nextExecution.IsInTheFuture()) return false;

            Log.Info($"{task.Name} is due.");

            if (task.Heartbeat is null) return true;

            if (task.Heartbeat.Value.AddMinutes(task.TimeoutInMinutes) > LocalTime.UtcNow)
            {
                Log.Info($"Skipped due background task '{task.Name}' because last attempt is still running on [{task.ExecutingInstance}] instance.");
                return false;
            }

            Log.Info($"Last attempt to run the background task '{task.Name}' on [{task.ExecutingInstance}] timed out.");
            return true;
        }

        internal static async Task<string> Run(bool force = false, string taskName = null)
        {
            Log.Info("Checking background tasks... @ " + LocalTime.UtcNow.ToString("HH:mm:ss"));

            var allTasks = await Db.GetList<IBackgourndTask>();

            var toRun = force
                ? allTasks.Where(a => string.IsNullOrWhiteSpace(taskName) || a.Name == taskName).ToArray()
                : allTasks.Where(ShouldRun).ToArray();

            if (toRun.None())
            {
                return "No task selected";
            }

            var batchTasks = Task.WhenAll(toRun.Select(t => Run(t)));

            var messages = new List<string>
            {
                $"Selected tasks : {string.Join(", ",toRun.Select(a=>a.Name))}"
            };

            try
            {
                await batchTasks;
            }
            catch (OperationCanceledException exception)
            {
                Log.Error(exception);
                messages.Add(exception.ToFullMessage());
            }
            catch (AggregateException exception)
            {
                Log.Error(exception);
                messages.Add(exception.ToFullMessage());
            }
            catch (Exception exception)
            {
                messages.Add(exception.ToFullMessage());
                if (batchTasks.Exception != null)
                {
                    Log.Error(batchTasks.Exception);
                    messages.Add(batchTasks.Exception.ToFullMessage());
                }
            }

            Log.Info($"Finished running {toRun.Select(c => c.Name).ToString(",")}.");
            messages.Add($"Finished running {toRun.Select(c => c.Name).ToString(",")}.");

            return string.Join("<br/>", messages);
        }

        static async Task Run(this IBackgourndTask task)
        {
            var start = LocalTime.Now;
            Log.Info($"Running background task '{task.Name}'");
            await Db.Update(task, x =>
            {
                x.Heartbeat = LocalTime.UtcNow;
                x.ExecutingInstance = Engine.ExecutionId;
            }).ConfigureAwait(false);

            try
            {
                await Engine.GetAction(task).ConfigureAwait(false);
                Log.Info($"Sucessfully ran background task '{task.Name}' in {LocalTime.Now.Subtract(start).ToNaturalTime()}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to run background task '{task.Name}' because: " + ex.ToFullMessage());
            }
            finally
            {
                await Db.Update(await Db.Reload(task), t =>
               {
                   t.LastExecuted = LocalTime.Now;
                   t.ExecutingInstance = null;
                   t.Heartbeat = null;
               }).ConfigureAwait(false);
            }
        }
    }
}