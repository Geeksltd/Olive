using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Olive.Aws
{
    public class EventBusQueue : IEventBusQueue
    {
        const int MAX_RETRY = 4;
        internal string QueueUrl;

        IAmazonSQS client;

        internal bool IsFifo => QueueUrl.EndsWith(".fifo");
        readonly Limiter Limiter = new Limiter(3000);

        public EventBusQueue(string queueUrl) => QueueUrl = queueUrl;

        public IAmazonSQS Client => client ?? Context.Current.GetOptionalService<IAmazonSQS>() ?? new AmazonSQSClient();

        /// <summary>
        ///  Creates and uses a new Aws Client in the specified region.
        /// </summary>
        public EventBusQueue Region(Amazon.RegionEndpoint region) => SetClient(new AmazonSQSClient(region));

        /// <summary>
        ///  Changes the Aws Client to the specified one. If used in AssumeRole configuration, make sure the Client object is refreshed.
        /// </summary>
        public EventBusQueue SetClient(IAmazonSQS client)
        {
            this.client = client;
            return this;
        }

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
                var eventBusMessage = JsonConvert.DeserializeObject<IEventBusMessage>(message);
                request.MessageDeduplicationId =eventBusMessage.DeduplicationId.Or(Guid.NewGuid().ToString("N"));
                request.MessageGroupId = eventBusMessage.MessageGroupId.Or("Default");
            }

            try
            {
                var response = await Client.SendMessageAsync(request);
                return response.MessageId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToFullMessage());
                throw new Exception("Failed to publish a message to queue: " + QueueUrl, ex);
            }
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
                request.Entries
                    .ForEach(message =>
                {
                    var eventBusMessage = JsonConvert.DeserializeObject<IEventBusMessage>(message.MessageBody);
                    message.MessageDeduplicationId =eventBusMessage.DeduplicationId.Or(Guid.NewGuid().ToString("N"));
                    message.MessageGroupId = eventBusMessage.MessageGroupId.Or("Default");
                });
            }

            await Limiter.Add(request.Entries.Count);

            var response = await Client.SendMessageBatchAsync(request);

            var successfuls = response.Successful.Select(m => m.MessageId).ToList();

            if (response.Failed.Any())
            {
                if (retry > MAX_RETRY)
                    throw new Exception("Failed to send all requests because : " + response.Failed.Select(f => f.Code).ToString(Environment.NewLine));

                Log.For(this)
                    .Warning($"Failed to send {response.Failed.Select(c => c.Message).ToLinesString()} because : {response.Failed.Select(c => c.Code).ToLinesString()}. Retrying for {retry}/{MAX_RETRY}.");

                var toSend = response.Failed.Select(f => f.Message);
                successfuls.AddRange(await PublishBatch(toSend, retry++));
            }

            return successfuls;
        }

        public void Subscribe(Func<string, Task> handler) => new Subscriber(this, handler).Start();

        public async Task PullAll(Func<string, Task> handler)
        {
            while (true)
            {
                // Instead of having multiple 10-secs waits it's better to have multiple 1-sec waits
                // Also this change will make it more Olive.Mvc.PerformanceMonitoringMiddleware friendly
                var batch = await PullBatch(timeoutSeconds: 1);
                if (batch.None()) return;

                foreach (var item in batch)
                {
                    await handler(item.RawMessage);
                    await item.Complete();
                }
            }
        }

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

        public Task Purge() => Client.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = QueueUrl });
    }
}
