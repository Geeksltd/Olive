using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Azure
{
    class Subscriber
    {
        public Func<string, Task> Handler { get; }
        EventBusQueue Queue;
        string QueueUrl => Queue.QueueUrl;
        Thread PollingThread;

        public Subscriber(EventBusQueue queue, Func<string, Task> handler)
        {
            Handler = handler;
            Queue = queue;
        }

        public void Start()
        {
            (PollingThread = new Thread(async () => await KeepPolling())).Start();
        }

        public Task PullAll() => KeepPolling(PullStrategy.UntilEmpty);

        async Task<List<KeyValuePair<string, ServiceBusReceivedMessage>>> FetchEvents(int? waitTimeSeconds)
        {
            var result = new List<KeyValuePair<string, ServiceBusReceivedMessage>>();

            foreach (var item in await Fetch(waitTimeSeconds))
            {
                result.Add(new KeyValuePair<string, ServiceBusReceivedMessage>(item.Body.ToString(), item));
            }

            return result;
        }

        async Task<IEnumerable<ServiceBusReceivedMessage>> Fetch(int? waitTimeSeconds)
        {
            try
            {
                await using (var context = CreateMessagingContext())
                {
                    return await context.Receiver.ReceiveMessagesAsync(Queue.MaxNumberOfMessages, waitTimeSeconds?.Seconds());
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch from Queue " + Queue.QueueUrl, ex);
            }
        }

        async Task<bool> Poll(int? waitTimeSeconds)
        {
            var messages = await FetchEvents(waitTimeSeconds);
            foreach (var item in messages)
            {
                try
                {
                    Log.For(this).Info("Fetched message : " + item.Value.Body);
                    await Handler(item.Key);

                    await using (var context = CreateMessagingContext())
                    {
                        await context.Receiver.CompleteMessageAsync(item.Value);
                    }
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

            return messages.Any();
        }

        private AzureMessagingContext CreateMessagingContext()
        {
            return new AzureMessagingContext(QueueUrl);
        }

        async Task KeepPolling(PullStrategy strategy = PullStrategy.KeepPulling, int? waitTimeSeconds = 10)
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