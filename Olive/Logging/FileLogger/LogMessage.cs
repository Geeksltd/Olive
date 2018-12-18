using System;

namespace Olive.Logging
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
        public int Severity { set; get; }
    }
}
