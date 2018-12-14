using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Logging
{
    public class EventBusLoggerProvider : BatchingLoggerProvider
    {
        string QueueUrl;

        public EventBusLoggerProvider(IOptions<EventBusLoggerOptions> options) : base(options)
        {
            QueueUrl = options.Value.QueueUrl;
        }

        public override Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token)
        {
            return EventBus.Queue(QueueUrl).Publish(new EventBusLoggerMessage { Messages = messages, Date = DateTime.Now });
        }
    }
}
