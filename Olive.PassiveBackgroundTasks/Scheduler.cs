using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.PassiveBackgroundTasks
{
    class Scheduler
    {
        internal static Task<IEnumerable<IBackgourndTask>> GetTasksToRun() =>
            Context.Current.Database().GetList<IBackgourndTask>().WhereAsync(i => i.TryPick());
    }
}
