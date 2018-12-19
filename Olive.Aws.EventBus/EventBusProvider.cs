using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Olive.Aws
{
    public class EventBusQueue : IEventBusQueue
    {
        internal string QueueUrl;
        internal AmazonSQSClient Client;

        public EventBusQueue(string queueUrl)
        {
            QueueUrl = queueUrl;
            Client = new AmazonSQSClient();
        }

        public async Task<string> Publish(IEventBusMessage message)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = QueueUrl,
                MessageBody = JsonConvert.SerializeObject(message)
            };

            if (QueueUrl.EndsWith(".fifo"))
            {
                request.MessageDeduplicationId = message.DeduplicationId;
                request.MessageGroupId = "Default";
            }

            var response = await Client.SendMessageAsync(request);
            return response.MessageId;
        }

        public void Subscribe<TMessage>(Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
        {
            new Subscriber<TMessage>(this, handler).Start();
        }

        public Task Purge()
        {
            return Client.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = QueueUrl });
        }
    }

    public class EventBusProvider : IEventBusQueueProvider
    {
        static ConcurrentDictionary<string, IEventBusQueue> Cache = new ConcurrentDictionary<string, IEventBusQueue>();

        public IEventBusQueue Provide(string queueUrl) => Cache.GetOrAdd(queueUrl, u => new EventBusQueue(u));
    }
}
