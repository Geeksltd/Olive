namespace Olive.Services.SMS
{
    public class SmsSendingEventArgs
    {
        public ISmsQueueItem Item { get; }

        public Exception Error { get; internal set; }

        public SmsSendingEventArgs(ISmsQueueItem item)
        {
            Item = item;
        }
    }
}
