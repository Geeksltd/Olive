using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Log.EventBus
{
    public class EventBusLoggerMessage : IEventBusMessage
    {
        /// <summary>
        /// list of Log Messages which publish in an interval
        /// </summary>
        public IEnumerable<LogMessage> LogMessages { set; get; }

        /// <summary>
        /// The DateTime when this message published
        /// </summary>
        public DateTime PublishDateTime { set; get; }

        public string DeduplicationId => Guid.NewGuid().ToString();
    }
}
