using System;
using System.Collections.Generic;

namespace Olive.Logging
{
    public class EventBusLoggerMessage : EventBusMessage
    {
        /// <summary>
        /// list of Log Messages which publish in an interval
        /// </summary>
        public IEnumerable<LogMessage> Messages { set; get; }

        public string Source { set; get; }

        public DateTime Date { set; get; }
    }
}
