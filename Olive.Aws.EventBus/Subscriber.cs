using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Subscriber
    {
        public Func<string, Task> Handler { get; }
        ReceiveMessageRequest Request;
        DeleteMessageRequest Receipt;
        EventBusQueue Queue;
        Thread PollingThread;

        public Subscriber(EventBusQueue queue, Func<string, Task> handler)
        {
            Handler = handler;
            Queue = queue;
        }

        public void Start()
        {
            Request = new ReceiveMessageRequest
            {
                QueueUrl = Queue.QueueUrl,
                WaitTimeSeconds = 10
            };

            Receipt = new DeleteMessageRequest { QueueUrl = Queue.QueueUrl };

            (PollingThread = new Thread(KeepPolling)).Start();
        }

        async Task<List<KeyValuePair<string, Message>>> FetchEvents()
        {
            var response = await Fetch();
            var result = new List<KeyValuePair<string, Message>>();

            foreach (var item in response.Messages)
            {
                result.Add(new KeyValuePair<string, Message>(item.Body, item));
            }

            return result;
        }

        async Task<ReceiveMessageResponse> Fetch()
        {
            try
            {
                return await Queue.Client.ReceiveMessageAsync(Request);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch from Queue " + Queue.QueueUrl, ex);
            }
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
                        await Queue.Client.DeleteMessageAsync(Receipt);
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