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

            if (exception != null) r.AppendLine(exception.ToString());

            Provider.AddMessage(timestamp, r.ToString(), (int)logLevel);
        }
    }
}
