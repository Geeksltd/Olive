using System;

namespace Olive.Logging
{
    public class BatchingLoggerOptions
    {
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The period after which logs will be flushed to the store. Default is one second.
        /// </summary>
        public TimeSpan FlushPeriod { get; set; } = 1.Seconds();

        /// <summary>
        /// The maximum number of log entries held in memory while waiting to be written. Defaults to
        /// <c>null</c>, meaning a built-in cap of 100,000; the queue is never unbounded.
        /// Reaching the cap does not block the thread that is logging: the newest entry is dropped and
        /// counted, and the count is reported into the log stream on the next flush.
        /// </summary>
        public int? BackgroundQueueSize { get; set; }

        /// <summary>
        /// The maximum number of events to include in a single batch. Use null for no limit.
        /// </summary>
        public int? BatchSize { get; set; } = 100;
    }
}