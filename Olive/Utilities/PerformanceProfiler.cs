using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Olive
{
    public class PerformanceProfiler : IDisposable
    {
        const int INDENTATION = 30;

        static readonly ConcurrentDictionary<string, ConcurrentList<TimeSpan>> Watchers = new();
        readonly ConcurrentList<TimeSpan> Watcher;
        readonly DateTime Start;
        readonly string Action;

        public PerformanceProfiler(string action = null, [CallerFilePath] string callerFile = null, [CallerMemberName] string method = null)
        {
            Action = callerFile?.Split('\\').Last().TrimEnd(".cs").WithSuffix(" -- ") + method + action.WithPrefix(" > ");
            Watcher = Watchers.GetOrAdd(action, x => new ConcurrentList<TimeSpan>());
            Start = DateTime.UtcNow;
        }

        public void Dispose()
        {
            Watcher.Add(DateTime.UtcNow.Subtract(Start));

            var average = Watcher.Average(x => x.TotalSeconds).Round(2);
            var max = Watcher.Max(x => x.TotalSeconds).Round(2);

            Debug.WriteLine(Action.PadRight(INDENTATION) +
                " Avg: " + average.ToString().PadRight(10) +
               " Max: " + max.ToString().PadRight(10));
        }
    }
}