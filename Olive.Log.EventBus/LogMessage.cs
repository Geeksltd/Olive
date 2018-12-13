using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Log.EventBus
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
    }
}
