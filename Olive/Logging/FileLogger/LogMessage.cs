using System;

namespace Olive.Logging
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
        public string ContextInfo { get; set; }
        public int Severity { set; get; }

        /// <summary>The stack trace, apart from the message rather than appended to it: the message
        /// carries what differs between two occurrences of one fault, the stack is the same every time,
        /// and welded together a consumer cannot keep one without a copy of the other. Null when nothing
        /// threw, and from a sink with no use for the distinction.</summary>
        public string Stack { set; get; }
    }
}