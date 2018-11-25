using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Subscriber<TMessage> : Agent where TMessage : IEventBusMessage
    {
        public Func<TMessage, Task> Handler { get; }
        ReceiveMessageRequest Request;
        DeleteMessageRequest Receipt;

        public Subscriber(string queueKey, Func<TMessage, Task> handler) : base(queueKey)
            => Handler = handler;

        public void Start()
        {
            Request = new ReceiveMessageRequest
            {
                QueueUrl = QueueUrl,
                WaitTimeSeconds = 10
            };

            Receipt = new DeleteMessageRequest
            {
                QueueUrl = QueueUrl
            };

            new Thread(KeepPolling).Start();
        }

        async Task<List<KeyValuePair<TMessage, Message>>> FetchEvents()
        {
            var result = new List<KeyValuePair<TMessage, Message>>();
            var response = await Client.ReceiveMessageAsync(Request);
            foreach (var item in response.Messages)
            {
                try
                {
                    var @event = JsonConvert.DeserializeObject<TMessage>(item.Body);
                    result.Add(new KeyValuePair<TMessage, Message>(@event, item));
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to deserialize event message to " +
                        typeof(TMessage).FullName + ":\r\n" + item.Body, ex);
                }
            }

            return result;
        }

        async void KeepPolling()
        {
            while (true)
            {
                foreach (var item in await FetchEvents())
                {
                    try
                    {
                        await Handler(item.Key);

                        Receipt.ReceiptHandle = item.Value.ReceiptHandle;
                        await Client.DeleteMessageAsync(Receipt);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to run queue event handler " +
                            Handler.Method.DeclaringType.FullName + "." +
                            Handler.Method.GetDisplayName(), ex);
                    }
                }
            }
        }
    }
}