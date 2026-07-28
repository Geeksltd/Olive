using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Olive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public EventBusLoggerProvider(IOptions<EventBusLoggerOptions> options, IConfiguration config) : base(options)
        {
            QueueUrl = (options?.Value?.QueueUrl).Or(() => config[ConfigKey]);
            if (QueueUrl.IsEmpty())
                throw new Exception("No queue url is specified in either EventBusLoggerOptions or under config key of " + ConfigKey);

            Source = (options?.Value?.Source).Or(() => Source = config[SourceKey]);
            if (Source.IsEmpty())
                throw new Exception("Source is specified in either EventBusLoggerOptions or under config key of " + SourceKey);
        }

        public override async Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken token)
        {
            var message = new EventBusLoggerMessage
            {
                Messages = messages.ToArray(),
                Date = DateTime.Now,
                Source = Source,
                DeduplicationId = GetDeduplicationId(messages)
            };

            try
            {
                // Typed, so the receiving service consumes it through the EventBusCommandMessage path
                // like any other command. Each entry carries its own reference code, not the envelope's.
                await EventBus.Queue<EventBusLoggerMessage>(QueueUrl).Publish(message);
            }
            catch (Exception ex)
            {
                // Rethrown, and nothing removed from `messages`: a publish that throws may still have
                // landed, so the whole batch is retried under the same deduplication id. The provider
                // drops it after its retry cap, which is the one attempt worth dumping to stdout.
                if (IsFinalAttempt)
                {
                    Console.WriteLine("Failed to publish the logs to the event bus at " + QueueUrl +
                        " after repeated attempts. Giving up on this batch; its entries follow.");
                    Console.WriteLine(ex.ToFullMessage());

                    foreach (var msg in messages)
                        Console.WriteLine(msg.Message + "\n----------\n");
                }
                else Console.WriteLine("Failed to publish the logs to the event bus at " + QueueUrl +
                    ". Keeping the batch and retrying.");

                throw;
            }
        }

        /// <summary>
        /// An id for this batch derived from its own content, in place of the fresh Guid EventBusMessage
        /// would hand out, so that a retried publish is recognisable as the same batch rather than a
        /// second one. Content only, never the clock: Date is stamped afresh on each attempt.
        /// </summary>
        string GetDeduplicationId(List<LogMessage> messages)
        {
            var identity = new StringBuilder(Source);

            foreach (var item in messages)
                identity.Append('\n').Append(item.Timestamp.UtcTicks)
                    .Append('|').Append(item.Severity)
                    .Append('|').Append(item.Message)
                    .Append('|').Append(item.ContextInfo);

            // Hex of SHA256: 64 characters, which is inside the 128 SQS allows for a
            // MessageDeduplicationId and uses only characters it accepts.
            using (var hasher = SHA256.Create())
                return BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(identity.ToString()))).Remove("-");
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => new EventBusLogger(this, categoryName);
    }
}