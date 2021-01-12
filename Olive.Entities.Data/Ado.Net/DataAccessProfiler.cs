using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides SQL profiling services.
    /// </summary>
    public class DataAccessProfiler
    {
        static ConcurrentBag<Watch> Watches = new ConcurrentBag<Watch>();

        static object SyncLock = new object();

        public static void Reset() => Watches = new ConcurrentBag<Watch>();

        internal static Watch Start(string command) => new Watch(command);

        [EscapeGCop("The rule does not apply to this as it's a system level calculation item.")]
        internal static void Complete(Watch watch)
        {
            watch.Duration = DateTime.UtcNow.Subtract(watch.Start);

            Watches.Add(watch);
        }

        internal class Watch
        {
            internal string Command;
            internal DateTime Start;
            internal TimeSpan Duration;

            [EscapeGCop("The rule does not apply to this as it's a system level calculation item.")]
            public Watch(string command)
            {
                Command = command.ToLines().ToString(" ");
                Start = DateTime.UtcNow;
            }
        }

        public class ReportRow
        {
            public string Command { get; internal set; }
            public int Calls { get; internal set; }
            public double Total { get; internal set; }
            public double Average { get; internal set; }
            public double Median { get; internal set; }
            public double Longest { get; internal set; }
        }

        /// <summary>
        /// To invoike this you can send a request to the application using http://.../cmd/db-profile?Mode=Snapshot
        /// </summary>
        /// <param name="snapshot">Determines whether the current log data should be removed (false) or kept for future combined future generated (true).</param>
        public static ReportRow[] GenerateReport(bool snapshot = false)
        {
            var items = Watches.ToArray().GroupBy(x => x.Command);

            if (!snapshot) Reset();

            return items.Select(item => new ReportRow
            {
                Command = item.Key,
                Calls = item.Count(),
                Total = item.Sum(x => x.Duration).TotalMilliseconds.Round(1),
                Average = item.Select(x => (x.Duration.TotalMilliseconds)).Average().Round(1),
                Median = item.Select(x => (int)(x.Duration.TotalMilliseconds * 100)).Median() * 0.01,
                Longest = item.Max(x => x.Duration).TotalMilliseconds.Round(1)
            }).ToArray();
        }
    }
}