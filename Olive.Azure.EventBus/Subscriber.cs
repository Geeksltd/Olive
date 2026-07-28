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
                // The scope covers the catch, not just the handler — see the AWS subscriber.
                using (Log.UseReference(EventBusExtensions.ReadReferenceCode(item.Key)))
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
                        var reportable = Log.For<Subscriber>().ReportFailure(ex,
                            "Failed to run queue event handler " + Handler.Method.DeclaringType.FullName +
                            "." + Handler.Method.GetDisplayName(), item.Key);

                        // A FIFO queue must not move on past a message it could not process, so the
                        // loop stops. The throw is only that signal, never a way of reporting.
                        if (Queue.IsFifo) throw reportable;
                    }
                }
            }

            return messages.Any();
        }

        private AzureMessagingContext CreateMessagingContext()
        {
            return new AzureMessagingContext(QueueUrl);
        }

        /// <summary>
        /// Polls the queue, either for ever or until it is empty. A drain that fails stops and tells its
        /// caller: swallowing it would spin for ever on the undeleted message at the head of a FIFO
        /// queue, and /olive/process-command would answer "Done" over a queue it never drained.
        /// A long-polling subscriber is meant to keep going — the next Fetch waits, so it is no spin.
        /// </summary>
        async Task KeepPolling(PullStrategy strategy = PullStrategy.KeepPulling, int? waitTimeSeconds = 10)
        {
            var draining = strategy == PullStrategy.UntilEmpty;
            var queueIsEmpty = false;

            do
            {
                try { queueIsEmpty = !await Poll(waitTimeSeconds); }
                catch (LoggedException) { if (draining) throw; }
                catch (Exception exception)
                {
                    Log.For<Subscriber>().Error(exception);

                    // Wrapped, so the caller is told the drain did not finish without reporting it again.
                    if (draining) throw new LoggedException(exception);
                }
            }
            while (!draining || !queueIsEmpty);
        }
    }
}