namespace Olive.Services.SMS
{
    using System;

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
