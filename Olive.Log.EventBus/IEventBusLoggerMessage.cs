using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Log.EventBus
{
    public class EventBusLoggerMessage : EventBusMessage
    {
        /// <summary>
        /// list of Log Messages which publish in an interval
        /// </summary>
        public IEnumerable<LogMessage> Messages { set; get; }

        /// <summary>
        /// The DateTime when this message published
        /// </summary>
        public DateTime Date { set; get; }
    }
}
