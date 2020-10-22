using Olive.Entities.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Olive.PassiveBackgroundTasks
{
    public class BackgroundProcessManager
    {
        public static Task Run() => Current.DoRun();
        public static BackgroundProcessManager Current { get; } = new BackgroundProcessManager();
        Dictionary<string, Func<Task>> Actions = new Dictionary<string, Func<Task>>();

        public async Task Register(string name, Expression<Func<Task>> action, int intervalInMinutes, int timeoutInMinutes = 5)
        {
            this.Logger().Info("Registering a background task for " + name);
            var context = Context.Current;
            var database = context.Database();

            var instance = (IBackgourndTask)((await database.GetList<IBackgourndTask>(b => b.Name == name)).SingleOrDefault() ?? context.GetService<IBackgourndTask>()).Clone();

            instance.Name = name;
            instance.IntervalInMinutes = intervalInMinutes;
            instance.TimeoutInMinutes = timeoutInMinutes;

            Actions[name] = action.Compile();

            await context.Database().Save(instance);

            this.Logger().Info("Registered a background task for " + name);
        }

        internal Task GetAction(IBackgourndTask task) => Actions[task.Name]();

        async Task DoRun()
        {
            this.Logger().Info("Checking background tasks ... @ " + LocalTime.UtcNow.ToString("HH:mm:ss"));

            var toRun = (await Scheduler.GetTasksToRun()).ToArray();
            var taskNames = toRun.Select(c => c.Name).ToString(",");

            this.Logger().Info($"Found {toRun.Count()} task(s){taskNames.WithWrappers(" [", "]")} to run.");

            await ExecutionEngine.RunAll(toRun);

            if (toRun.Any())
                this.Logger().Info($"Finished running {taskNames}.");
        }
    }
}
