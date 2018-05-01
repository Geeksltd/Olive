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
        /// Gets or sets the maximum size of the background log message queue or null for no limit.
        /// After maximum queue size is reached log event sink would start blocking.
        /// Defaults to <c>null</c>.
        /// </summary>
        public int? BackgroundQueueSize { get; set; }

        /// <summary>
        /// The maximum number of events to include in a single batch. Use null for no limit.
        /// </summary>
        public int? BatchSize { get; set; } = 100;
    }
}
