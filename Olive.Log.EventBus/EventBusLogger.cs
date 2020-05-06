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

            Console.WriteLine("EventBusLogger: Log");

            var r = new StringBuilder();

            r.AppendLine(formatter(state, exception));

            if (exception != null) r.AppendLine(exception.ToFullMessage());

            Console.WriteLine("EventBusLogger: sending log to provider");
            Provider.AddMessage(timestamp, r.ToString(), (int)logLevel);
        }
    }
}
