using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Olive
{
    public class PerformanceProfiler : IDisposable
    {
        static readonly ConcurrentDictionary<string, ConcurrentList<TimeSpan>> Watchers =
            new ConcurrentDictionary<string, ConcurrentList<TimeSpan>>();
        readonly ConcurrentList<TimeSpan> Watcher;
        readonly DateTime Start;
        readonly string Action;

        public PerformanceProfiler(string action, int millisecondAccuracy = 2)
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

            Console.WriteLine(Action.PadRight(30) +
                " Avg: " + average.ToString().PadRight(10) +
               " Max: " + max.ToString().PadRight(10));
        }
    }
}
