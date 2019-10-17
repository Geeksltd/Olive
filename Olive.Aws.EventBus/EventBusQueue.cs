using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    public class EventBusQueue : IEventBusQueue
    {
        internal string QueueUrl;
        internal AmazonSQSClient Client;
        internal bool IsFifo => QueueUrl.EndsWith(".fifo");

        /// <summary>
        ///     Gets and sets the property MaxNumberOfMessages.
        ///     The maximum number of messages to return. Amazon SQS never returns more messages
        ///     than this value (however, fewer messages might be returned). Valid values: 1
        ///     to 10. Default: 1.
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = 10;

        /// <summary>
        ///     Gets and sets the property VisibilityTimeout.
        ///     The duration (in seconds) that the received messages are hidden from subsequent
        ///     retrieve requests after being retrieved by a ReceiveMessage request.
        /// </summary>
        public int VisibilityTimeout { get; set; } = 300;

        public EventBusQueue(string queueUrl)
        {
            QueueUrl = queueUrl;
            Client = new AmazonSQSClient();
        }

        public async Task<string> Publish(string message)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = QueueUrl,
                MessageBody = message,
            };

            if (IsFifo)
            {
                request.MessageDeduplicationId = JsonConvert.DeserializeObject<JObject>(message)["DeduplicationId"]?.ToString();
                request.MessageGroupId = "Default";
            }

            var response = await Client.SendMessageAsync(request);
            return response.MessageId;
        }

        public void Subscribe(Func<string, Task> handler) => new Subscriber(this, handler).Start();

        public async Task<QueueMessageHandle> Pull(int timeoutSeconds = 10)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = QueueUrl,
                WaitTimeSeconds = timeoutSeconds,
                MaxNumberOfMessages = MaxNumberOfMessages,
                VisibilityTimeout = VisibilityTimeout,
            };

            var response = await Client.ReceiveMessageAsync(request);
            foreach (var item in response.Messages)
            {
                var receipt = new DeleteMessageRequest { QueueUrl = QueueUrl, ReceiptHandle = item.ReceiptHandle };
                return new QueueMessageHandle(item.Body, () => Client.DeleteMessageAsync(receipt));
            }

            return null;
        }

        public Task Purge()
        {
            return Client.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = QueueUrl });
        }
    }
}
