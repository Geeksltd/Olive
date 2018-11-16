namespace Olive.SMS
{
    using System;

    public class SmsSendingEventArgs
    {
        public ISmsMessage Item { get; }

        public Exception Error { get; internal set; }

        public SmsSendingEventArgs(ISmsMessage item)
        {
            Item = item;
        }
    }
}
