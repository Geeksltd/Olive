using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Aws
{
    public class EventBusQueue : IEventBusQueue
    {
        internal string QueueUrl;
        internal AmazonSQSClient Client;
        internal bool IsFifo => QueueUrl.EndsWith(".fifo");
        readonly Limiter Limiter = new Limiter(3000);

        /// <summary>
        ///     Gets and sets the property MaxNumberOfMessages.
        ///     The maximum number of messages to return. Amazon SQS never returns more messages
        ///     than this value (however, fewer messages might be returned). Valid values: 1
        ///     to 10. Default: 1.
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = Config.Get("Aws:EventBusQueue:MaxNumberOfMessages", 10);

        /// <summary>
        ///     Gets and sets the property VisibilityTimeout.
        ///     The duration (in seconds) that the received messages are hidden from subsequent
        ///     retrieve requests after being retrieved by a ReceiveMessage request.
        /// </summary>
        public int VisibilityTimeout { get; set; } = Config.Get("Aws:EventBusQueue:VisibilityTimeout", 300);

        public EventBusQueue(string queueUrl)
        {
            QueueUrl = queueUrl;
            Client = new AmazonSQSClient();
        }

        public async Task<string> Publish(string message)
        {
            await Limiter.Add(1);

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

        public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages)
        {
            var request = new SendMessageBatchRequest
            {
                QueueUrl = QueueUrl,
            };

            messages.Do(message =>
                request.Entries.Add(new SendMessageBatchRequestEntry
                {
                    MessageBody = message,
                    Id = JsonConvert.DeserializeObject<JObject>(message)["Id"]?.ToString()
                        ?? Guid.NewGuid().ToString(),
                }));

            if (IsFifo)
            {
                request.Entries.ForEach(message =>
                {
                    message.MessageDeduplicationId =
                        JsonConvert.DeserializeObject<JObject>(message.MessageBody)["DeduplicationId"]?.ToString();
                    message.MessageGroupId = "Default";
                });
            }

            await Limiter.Add(request.Entries.Count);

            var response = await Client.SendMessageBatchAsync(request);

            return response.Successful.Select(m => m.MessageId);
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
