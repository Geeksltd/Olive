using System;

namespace Olive.Logging
{
    struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
    }
}
