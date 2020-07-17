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
        const int MAX_RETRY = 4;
        internal string QueueUrl;
        internal AmazonSQSClient Client => new AmazonSQSClient();
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

        public Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages) => PublishBatch(messages, 0);

        public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages, int retry = 0)
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

            var successfuls = response.Successful.Select(m => m.MessageId).ToList();

            if (response.Failed.Any())
            {
                if (retry > MAX_RETRY)
                    throw new Exception("Failed to send all requests because : " + response.Failed.Select(f => f.Code).ToString(Environment.NewLine));

                Log.For(this).Warning($"Failed to send {response.Failed.Select(c => c.Message).ToLinesString()} because : {response.Failed.Select(c => c.Code).ToLinesString()}. Retrying for {retry}/{MAX_RETRY}.");

                var toSend = response.Failed.Select(f => f.Message);
                successfuls.AddRange(await PublishBatch(toSend, retry++));
            }

            return successfuls;
        }

        public void Subscribe(Func<string, Task> handler) => new Subscriber(this, handler).Start();

        public Task PullAll(Func<string, Task> handler) => new Subscriber(this, handler).PullAll();

        public async Task<IEnumerable<QueueMessageHandle>> PullBatch(int timeoutSeconds = 10, int? maxNumerOfMessages = null)
        {
            var result = new List<QueueMessageHandle>();
            var request = new ReceiveMessageRequest
            {
                QueueUrl = QueueUrl,
                WaitTimeSeconds = timeoutSeconds,
                MaxNumberOfMessages = maxNumerOfMessages ?? MaxNumberOfMessages,
                VisibilityTimeout = VisibilityTimeout,
            };

            var response = await Client.ReceiveMessageAsync(request);
            foreach (var item in response.Messages)
            {
                var receipt = new DeleteMessageRequest { QueueUrl = QueueUrl, ReceiptHandle = item.ReceiptHandle };
                result.Add(new QueueMessageHandle(item.Body, item.MessageId, () => Client.DeleteMessageAsync(receipt)));
            }
            
            return result;
        }

        public Task<QueueMessageHandle> Pull(int timeoutSeconds = 10) => PullBatch(timeoutSeconds, 1).FirstOrDefault();

        public Task Purge()
        {
            return Client.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = QueueUrl });
        }
    }
}