using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Logging
{
    public class EventBusLoggerProvider : BatchingLoggerProvider
    {
        const string ConfigKey = "Logging:EventBus:QueueUrl";
        IEventBusQueue Queue;
        string QueueUrl;

        public EventBusLoggerProvider(IOptions<EventBusLoggerOptions> options,
            IConfiguration config) : base(options)
        {
            QueueUrl = options?.Value?.QueueUrl;

            if (QueueUrl.IsEmpty())
                QueueUrl = config.GetValue<string>(ConfigKey);

            if (QueueUrl.IsEmpty())
                throw new Exception("No queue url is specified in either EventBusLoggerOptions or under config key of " + ConfigKey);
        }

        public override Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token)
        {
            return EventBus.Queue(QueueUrl).Publish(new EventBusLoggerMessage { Messages = messages, Date = DateTime.Now });
        }
    }
}
