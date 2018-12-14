namespace Olive.Logging
{
    public class EventBusLoggerOptions : BatchingLoggerOptions
    {
        public string QueueUrl { set; get; }
    }
}
