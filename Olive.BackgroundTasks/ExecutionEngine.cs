using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.DistributedBackgroundTasks
{
    class ExecutionEngine
    {
        internal static Guid Id { get; } = Guid.NewGuid();
        internal static Task RunAll(IEnumerable<IBackgourndTask> tasks) =>
            Task.WhenAll(tasks.Select(t => TaskExecution.Run(t)));
    }
}
