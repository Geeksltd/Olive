using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Olive;
using Microsoft.Extensions.Logging;

namespace Olive.Logging
{
    public class EventBusLoggerProvider : BatchingLoggerProvider, ILoggerProvider
    {
        const string ConfigKey = "Logging:EventBus:QueueUrl";
        const string SourceKey = "Logging:EventBus:Source";
        IEventBusQueue Queue;
        string QueueUrl;
        string Source;

        public EventBusLoggerProvider(IOptions<EventBusLoggerOptions> options,
            IConfiguration config) : base(options)
        {
            QueueUrl = options?.Value?.QueueUrl;

            if (QueueUrl.IsEmpty())
                QueueUrl = config.GetValue<string>(ConfigKey);

            if (QueueUrl.IsEmpty())
                throw new Exception("No queue url is specified in either EventBusLoggerOptions or under config key of " + ConfigKey);

            Source = options?.Value?.Source;

            if (Source.IsEmpty())
                Source = config.GetValue<string>(SourceKey);

            if (Source.IsEmpty())
                throw new Exception("Source is specified in either EventBusLoggerOptions or under config key of " + SourceKey);
        }

        public override Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token)
        {
            return Olive.EventBus.Queue(QueueUrl).Publish(new EventBusLoggerMessage { Messages = messages, Date = DateTime.Now, Source = Source });
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => new EventBusLogger(this, categoryName);
    }
}
