namespace Olive.Logging
{
    public class EventBusLoggerOptions : BatchingLoggerOptions
    {
        public string QueueUrl { set; get; }

        /// <summary>
        /// the Source: Service,...
        /// </summary>
        public string Source { set; get; }
    }
}
