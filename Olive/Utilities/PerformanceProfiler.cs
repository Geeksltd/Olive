using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Olive
{
    public class PerformanceProfiler : IDisposable
    {
        const int INDENTATION = 30;

        static readonly ConcurrentDictionary<string, ConcurrentList<TimeSpan>> Watchers =
new();
        readonly ConcurrentList<TimeSpan> Watcher;
        readonly DateTime Start;
        readonly string Action;

        public PerformanceProfiler(string action)
        {
            Action = action;
            Watcher = Watchers.GetOrAdd(action, x => new ConcurrentList<TimeSpan>());
            Start = DateTime.UtcNow;
        }

        public void Dispose()
        {
            Watcher.Add(DateTime.UtcNow.Subtract(Start));

            var average = Watcher.Average(x => x.TotalSeconds).Round(2);
            var max = Watcher.Max(x => x.TotalSeconds).Round(2);

            Console.WriteLine(Action.PadRight(INDENTATION) +
                " Avg: " + average.ToString().PadRight(10) +
               " Max: " + max.ToString().PadRight(10));
        }
    }
}