using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Olive.Logging
{
    public class EventBusLogger : Olive.Logging.BatchingLogger
    {
        public EventBusLogger(EventBusLoggerProvider provider, string category)
            : base(provider, category)
        {
        }

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
