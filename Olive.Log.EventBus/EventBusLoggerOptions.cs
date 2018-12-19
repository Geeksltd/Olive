namespace Olive.Logging
{
    public class EventBusLoggerOptions : BatchingLoggerOptions
    {
        public string QueueUrl { set; get; }

        /// <summary>
        /// The Source of the log. This is normally the name of the Microservice that generated the log.
        /// </summary>
        public string Source { set; get; }
    }
}
