using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using azureMessaging = Azure.Messaging.ServiceBus;

namespace Olive.Azure
{
    public class EventBusQueue : IEventBusQueue
    {
        const int MAX_RETRY = 4;
        internal string QueueUrl;
        internal bool IsFifo => QueueUrl.EndsWith(".fifo");
        readonly Limiter Limiter = new Limiter(3000);

        /// <summary>
        ///     Gets and sets the property MaxNumberOfMessages.
        ///     The maximum number of messages to return. Amazon SQS never returns more messages
        ///     than this value (however, fewer messages might be returned). Valid values: 1
        ///     to 10. Default: 1.
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = Config.Get("Azure:EventBusQueue:MaxNumberOfMessages", 10);

        public EventBusQueue(string queueUrl)
        {
            QueueUrl = queueUrl;
        }

        public async Task<string> Publish(string message)
        {
            await Limiter.Add(1);

            await using (var context = new AzureMessagingContext(QueueUrl))
            {
                var msg = new azureMessaging.ServiceBusMessage(message);

                await context.Sender.SendMessageAsync(msg);

                return msg.MessageId;
            }
        }

        public Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages) => PublishBatch(messages, 0);

        public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages, int retry = 0)
        {
            await Limiter.Add(messages.Count());

            await using (var context = new AzureMessagingContext(QueueUrl))
            {
                var batch = await context.Sender.CreateMessageBatchAsync();
                var messageObjects = messages.Select(c => new azureMessaging.ServiceBusMessage(c)).ToArray();
                messageObjects.Do(m => batch.TryAddMessage(m));

                await context.Sender.SendMessagesAsync(batch);

                return messageObjects.Select(c => c.MessageId);
            }
        }

        public void Subscribe(Func<string, Task> handler) => new Subscriber(this, handler).Start();

        public Task PullAll(Func<string, Task> handler) => new Subscriber(this, handler).PullAll();

        public async Task<IEnumerable<QueueMessageHandle>> PullBatch(int timeoutSeconds = 10, int? maxNumerOfMessages = null)
        {
            var result = new List<QueueMessageHandle>();
            await using (var context = new AzureMessagingContext(QueueUrl))
            {
                var receiver = context.Receiver;

                foreach (var message in await receiver.ReceiveMessagesAsync(MaxNumberOfMessages, timeoutSeconds.Seconds()))
                    result.Add(new QueueMessageHandle(message.Body.ToString(), message.MessageId, () => Task.CompletedTask));
            }

            return result;
        }

        public Task<QueueMessageHandle> Pull(int timeoutSeconds = 10) => PullBatch(timeoutSeconds, 1).FirstOrDefault();

        public async Task Purge()
        {
            await using (var context = new AzureMessagingContext(QueueUrl))
            {
                var receiver = context.Purger;

                while (await receiver.PeekMessageAsync() != null)
                    await receiver.ReceiveMessagesAsync(MaxNumberOfMessages);
            }
        }
    }
}