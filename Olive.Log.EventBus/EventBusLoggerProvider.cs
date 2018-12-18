using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olive.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Log.EventBus
{
    public class EventBusLoggerProvider : Olive.Logging.BatchingLoggerProvider
    {
        string QueueUrl;
        string Source;

        public EventBusLoggerProvider(IOptions<EventBusLoggerOptions> options) : base(options)
        {
            QueueUrl = options.Value.QueueUrl;
            Source = options.Value.Source;
        }

        public override Task WriteMessagesAsync(IEnumerable<Logging.LogMessage> messages, CancellationToken token)
        {
            return Olive.EventBus.Queue(QueueUrl).Publish(new EventBusLoggerMessage() { LogMessages = messages, PublishDateTime = DateTime.Now, Source = Source });
        }
    }
}
