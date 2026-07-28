using System;

namespace Olive
{
    /// <summary>
    /// A failure that has already been reported to the log, thrown on only so that the caller can act
    /// on it — and telling that caller not to report it a second time.
    /// <para>
    /// Where a failure is logged decides which reference code it is filed under. By the time an
    /// exception reaches a queue subscriber, the handler's scope has unwound, so a subscriber that
    /// logged it again would file the same failure under a second, unrelated code.
    /// </para>
    /// </summary>
    public class LoggedException : Exception
    {
        public LoggedException(Exception inner) : base(inner?.Message, inner) { }
    }
}
