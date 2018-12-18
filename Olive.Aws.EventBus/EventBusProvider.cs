using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
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
                MessageBody = JsonConvert.SerializeObject(message),
                MessageDeduplicationId = message.DeduplicationId,
                MessageGroupId = "Default"
            };

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
        public IEventBusQueue Provide(string queueUrl) => new EventBusQueue(queueUrl);
    }
}
