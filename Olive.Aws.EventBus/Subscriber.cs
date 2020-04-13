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
            Request = new ReceiveMessageRequest
            {
                QueueUrl = Queue.QueueUrl,
                MaxNumberOfMessages = Queue.MaxNumberOfMessages,
                VisibilityTimeout = Queue.VisibilityTimeout,
                WaitTimeSeconds = 10
            };

            Receipt = new DeleteMessageRequest { QueueUrl = Queue.QueueUrl };
        }

        public void Start()
        {
            (PollingThread = new Thread(async () => await KeepPolling())).Start();
        }

        public Task PullAll() => KeepPolling(PullStrategy.UntilEmpty);

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
            catch (TaskCanceledException)
            {
                return new ReceiveMessageResponse { Messages = new List<Message>() };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch from Queue " + Queue.QueueUrl, ex);
            }
        }

        async Task<bool> Poll()
        {
            var messages = await FetchEvents();
            foreach (var item in messages)
            {
                try
                {
                    await Handler(item.Key);

                    Receipt.ReceiptHandle = item.Value.ReceiptHandle;
                    await Queue.Client.DeleteMessageAsync(Receipt);
                }
                catch (Exception ex)
                {
                    var exception = new Exception("Failed to run queue event handler " +
                        Handler.Method.DeclaringType.FullName + "." +
                        Handler.Method.GetDisplayName(), ex);

                    if (Queue.IsFifo)
                        throw exception;
                    else
                        Log.For<Subscriber>().Error(exception);
                }
            }

            return messages.Count == Queue.MaxNumberOfMessages;
        }

        async Task KeepPolling(PullStrategy strategy = PullStrategy.KeepPulling)
        {
            var queueIsEmpty = false;
            do
            {
                try
                {
                    queueIsEmpty = (await Poll() == false);
                }
                catch (Exception exception)
                {
                    Log.For<Subscriber>().Error(exception);
                }
            }
            while (strategy == PullStrategy.KeepPulling || !queueIsEmpty);
        }
    }
}