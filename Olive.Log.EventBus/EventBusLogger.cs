using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Olive.Logging
{
    public class EventBusLogger : BatchingLogger
    {
        public EventBusLogger(EventBusLoggerProvider provider, string category) : base(provider, category) { }

        public override void Log<TState>(DateTimeOffset timestamp, LogLevel logLevel, EventId _, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var r = new StringBuilder();

            r.AppendLine(formatter(state, exception));

            // The stack travels apart, unlike the file logger above: this one feeds a store that keeps one
            // row per distinct fault and counts occurrences onto it. The message is what differs between
            // two of them, the stack is what does not, and joined together the store cannot keep one
            // occurrence's message without a copy of the stack beside it.
            if (exception != null) r.AppendLine(exception.ToLogString());

            var message = r.ToString();
            string stack = null;

            // Built whole and then cut, not built without the stack: ToFullMessage's includeStackTrace
            // also governs the rule it draws between inner exceptions, so a message built without it
            // would no longer read as it always has — and a store grouping occurrences by that text
            // would split every group it holds the moment one service took this version.
            var at = message.IndexOf(OliveExtensions.StackTracePrefix, StringComparison.Ordinal);

            if (at >= 0)
            {
                // Null rather than empty, so "sends no stacks" cannot be mistaken for "had none".
                stack = message.Substring(at + OliveExtensions.StackTracePrefix.Length).TrimEnd().OrNullIfEmpty();
                message = message.Substring(0, at).TrimEnd();
            }

            var contextInfo = Olive.Log.ContextProvider?.Invoke();

            Provider.AddMessage(timestamp, message, (int)logLevel, contextInfo, stack);
        }
    }
}
