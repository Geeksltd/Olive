using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Logging
{
    public abstract class BatchingLoggerProvider : ILoggerProvider
    {
        readonly List<LogMessage> CurrentBatch = new List<LogMessage>();
        readonly TimeSpan Interval;
        int? QueueSize, BatchSize;

        BlockingCollection<LogMessage> MessageQueue;
        Task OutputTask;
        CancellationTokenSource CancellationTokenSource;

        protected BatchingLoggerProvider(IOptions<BatchingLoggerOptions> options)
        {
            // NOTE: Only IsEnabled is monitored

            var loggerOptions = options.Value;
            if (loggerOptions.BatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.BatchSize));

            if (loggerOptions.FlushPeriod <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.FlushPeriod));

            Interval = loggerOptions.FlushPeriod;
            BatchSize = loggerOptions.BatchSize;
            QueueSize = loggerOptions.BackgroundQueueSize;

            Start();
        }

        public abstract Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token);

        async Task ProcessLogQueue(object _)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                var limit = BatchSize ?? int.MaxValue;

                while (limit > 0 && MessageQueue.TryTake(out var message))
                {
                    CurrentBatch.Add(message);
                    limit--;
                }

                if (CurrentBatch.Any())
                {
                    try { await WriteMessagesAsync(CurrentBatch, CancellationTokenSource.Token); }
                    catch
                    {
                        // ignored
                    }

                    CurrentBatch.Clear();
                }

                await Task.Delay(Interval, CancellationTokenSource.Token);
            }
        }

        public void AddMessage(DateTimeOffset timestamp, string message, int severity = 0)
        {
            if (!MessageQueue.IsAddingCompleted)
            {
                try
                {
                    MessageQueue.Add(new LogMessage { Message = message, Timestamp = timestamp, Severity = severity }, CancellationTokenSource.Token);
                }
                catch
                {
                    // cancellation token canceled or CompleteAdding called
                }
            }
        }

        void Start()
        {
            if (QueueSize == null)
                MessageQueue = new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>());
            else MessageQueue = new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>(), QueueSize.Value);

            CancellationTokenSource = new CancellationTokenSource();
            OutputTask = Task.Factory.StartNew(ProcessLogQueue, null, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
            MessageQueue.CompleteAdding();

            try { OutputTask.Wait(Interval); }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.IsSingle() && ex.InnerExceptions[0] is TaskCanceledException) { }
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => new BatchingLogger(this, categoryName);
    }
}