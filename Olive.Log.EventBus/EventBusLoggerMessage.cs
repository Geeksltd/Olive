using System;

namespace Olive.Logging
{
    public class EventBusLoggerMessage : EventBusMessage
    {
        public LogMessage[] Messages { set; get; }

        public string Source { set; get; }

        public DateTime Date { set; get; }
    }
}
