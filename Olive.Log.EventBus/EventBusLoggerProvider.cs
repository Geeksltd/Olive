using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Olive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Olive.Logging
{
    public class EventBusLoggerProvider : BatchingLoggerProvider, ILoggerProvider
    {
        const string ConfigKey = "Logging:EventBus:QueueUrl";
        const string SourceKey = "Logging:EventBus:Source";
        string QueueUrl, Source;
        bool IsEnabled = true;

        public EventBusLoggerProvider(IOptions<EventBusLoggerOptions> options, IConfiguration config) : base(options)
        {
            QueueUrl = (options?.Value?.QueueUrl).Or(() => config[ConfigKey]);
            if (QueueUrl.IsEmpty())
                throw new Exception("No queue url is specified in either EventBusLoggerOptions or under config key of " + ConfigKey);

            Source = (options?.Value?.Source).Or(() => Source = config[SourceKey]);
            if (Source.IsEmpty())
                throw new Exception("Source is specified in either EventBusLoggerOptions or under config key of " + SourceKey);
        }

        public override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token)
        {

            var message = new EventBusLoggerMessage
            {
                Messages = messages.ToArray(),
                Date = DateTime.Now,
                Source = Source
            };

            try
            {
                Console.WriteLine("EventBusLoggerProvider: sending message to " + QueueUrl);
                await EventBus.Queue(QueueUrl).Publish(message);
                Console.WriteLine("EventBusLoggerProvider: sent message to " + QueueUrl);
            }
            catch (Exception ex)
            {
                IsEnabled = false;
                Console.WriteLine("Fatal error: Failed to publish the logs to the event bus.");
                Console.WriteLine(ex.ToFullMessage());
            }
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => new EventBusLogger(this, categoryName);
    }
}