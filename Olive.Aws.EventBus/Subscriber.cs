using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Subscriber
    {
        public Func<string, Task> Handler { get; }
        EventBusQueue Queue;
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

        public Task PullAll() => KeepPolling(PullStrategy.UntilEmpty, waitTimeSeconds: 10);

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
                // The scope covers the catch, not just the handler: the handler's own scope has unwound
                // by the time we log its failure, and that entry is usually the only record of it.
                using (Log.UseReference(EventBusExtensions.ReadReferenceCode(item.Key)))
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
                        var reportable = Log.For<Subscriber>().ReportFailure(ex, Handler.Method, item.Key);

                        // A FIFO queue must not move on past a message it could not process, so the
                        // loop stops. The throw is only that signal, never a way of reporting.
                        if (Queue.IsFifo) throw reportable;
                    }
                }
            }

            return messages.Any();
        }

        /// <summary>
        /// Polls the queue, either for ever or until it is empty. A drain that fails stops and tells its
        /// caller: swallowing it would spin for ever on the undeleted message at the head of a FIFO
        /// queue, and would report success to the request that asked for the queue to be emptied.
        /// A long-polling subscriber is meant to keep going — the next Fetch waits, so it is no spin.
        /// </summary>
        async Task KeepPolling(PullStrategy strategy = PullStrategy.KeepPulling, int waitTimeSeconds = 10)
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