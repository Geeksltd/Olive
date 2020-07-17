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
        //ReceiveMessageRequest Request;
        //DeleteMessageRequest Receipt;
        EventBusQueue Queue;
        Thread PollingThread;

        public Subscriber(EventBusQueue queue, Func<string, Task> handler)
        {
            Handler = handler;
            Queue = queue;

            // Receipt = new DeleteMessageRequest { QueueUrl = Queue.QueueUrl };
        }

        public void Start()
        {
            (PollingThread = new Thread(async () => await KeepPolling())).Start();
        }

        public Task PullAll() => KeepPolling(PullStrategy.UntilEmpty, waitTimeSeconds: 0);

        async Task<List<KeyValuePair<string, Message>>> FetchEvents(int waitTimeSeconds)
        {
            var response = await Fetch(waitTimeSeconds);
            var result = new List<KeyValuePair<string, Message>>();

            foreach (var item in response.Messages)
            {
                result.Add(new KeyValuePair<string, Message>(item.Body, item));
            }

            return result;
        }

        async Task<ReceiveMessageResponse> Fetch(int waitTimeSeconds)
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = Queue.QueueUrl,
                    MaxNumberOfMessages = Queue.MaxNumberOfMessages,
                    VisibilityTimeout = Queue.VisibilityTimeout,
                    WaitTimeSeconds = waitTimeSeconds //10
                };

                return await Queue.Client.ReceiveMessageAsync(request);
            }
            catch (TaskCanceledException)
            {
                return new ReceiveMessageResponse { Messages = new List<Message>() };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch from Queue " + Queue.QueueUrl, ex);
            }
        }

        async Task<bool> Poll(int waitTimeSeconds)
        {
            var messages = await FetchEvents(waitTimeSeconds);
            foreach (var item in messages)
            {
                try
                {
                    var receipt = new DeleteMessageRequest { QueueUrl = Queue.QueueUrl };
                    Log.For(this).Info("Fetched message : " + item.Value.Body);
                    await Handler(item.Key);

                    receipt.ReceiptHandle = item.Value.ReceiptHandle;
                    await Queue.Client.DeleteMessageAsync(receipt);
                }
                catch (Exception ex)
                {
                    var exception = new Exception("Failed to run queue event handler " +
                        Handler.Method.DeclaringType.FullName + "." +
                        Handler.Method.GetDisplayName() +
                        "message: " + item.Key?.ToJsonText(), ex);

                    if (Queue.IsFifo)
                        throw exception;
                    else
                        Log.For<Subscriber>().Error(exception);
                }
            }

            return messages.Count > 0;
        }

        async Task KeepPolling(PullStrategy strategy = PullStrategy.KeepPulling, int waitTimeSeconds = 10)
        {
            var queueIsEmpty = false;
            do
            {
                try { queueIsEmpty = !await Poll(waitTimeSeconds); }
                catch (Exception exception) { Log.For<Subscriber>().Error(exception); }
            }
            while (strategy == PullStrategy.KeepPulling || !queueIsEmpty);
        }
    }
}