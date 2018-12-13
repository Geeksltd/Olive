using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Log.EventBus
{
    public class EventBusLoggerOptions : Olive.Logging.BatchingLoggerOptions
    {
        /// <summary>
        /// the queue url
        /// </summary>
        public string QueueUrl { set; get; }
    }
}
