using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.PassiveBackgroundTasks
{
    class ExecutionEngine
    {
        internal static Guid Id { get; } = Guid.NewGuid();
        internal static async Task RunAll(IEnumerable<IBackgourndTask> tasks)
        {
            await Task.WhenAll(tasks.Select(async t =>
            {
                try
                {
                    await TaskExecution.Run(t);
                }
                catch (Exception ex)
                {
                    Log.For<ExecutionEngine>().Error(ex, $"Failed to run background task : {t.Name} because : " + ex.ToFullMessage());
                }
            }));
        }
    }
}
