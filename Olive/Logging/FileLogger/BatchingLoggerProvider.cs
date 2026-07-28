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
        readonly List<LogMessage> CurrentBatch = new();
        readonly TimeSpan Interval;
        readonly int? QueueSize, BatchSize;

        BlockingCollection<LogMessage> MessageQueue;
        Task OutputTask;
        CancellationTokenSource CancellationTokenSource;

        // Consecutive failed flushes of the same batch tolerated before it is dropped. Until then the
        // batch is retained and retried, so a transient sink outage does not lose logs.
        const int MaxFailedFlushesBeforeDrop = 10;
        int FailedFlushes;

        /// <summary>
        /// True while the batch being written is on its last attempt: fail this one and it is dropped.
        /// A provider with a fallback (stdout, say) should spend it once, here, rather than on every
        /// retry — ten copies of a batch in the one stream still working helps nobody.
        /// </summary>
        protected bool IsFinalAttempt => FailedFlushes >= MaxFailedFlushesBeforeDrop - 1;

        const int DefaultBackgroundQueueSize = 100_000;

        // Entries dropped because the queue was full, reported on the next flush by AppendDropNotice.
        long DroppedCount;

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

        /// <summary>
        /// Writes a batch to the sink. Throw to report that some of it did not land; the batch is then
        /// kept and passed again on the next cycle rather than dropped.
        /// <para>
        /// A batch is retried whole, so an implementation writing it in parts MUST remove each part from
        /// <paramref name="messages"/> as soon as that part is durable, or a retry writes it twice.
        /// Removing nothing and throwing is right when the write's fate is unknown (a publish that timed
        /// out): give the sink a stable identity for the batch so it can recognise the repeat.
        /// </para>
        /// </summary>
        public abstract Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken token);

        async Task ProcessLogQueue(object _)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                // Only pull new messages once the previous batch is written, so a sink that is down
                // cannot grow the retained batch beyond one batch's worth.
                if (CurrentBatch.None())
                {
                    var limit = BatchSize ?? int.MaxValue;

                    while (limit > 0 && MessageQueue.TryTake(out var message))
                    {
                        CurrentBatch.Add(message);
                        limit--;
                    }

                    AppendDropNotice();
                }

                if (CurrentBatch.Any())
                {
                    var countBefore = CurrentBatch.Count;

                    try
                    {
                        await WriteMessagesAsync(CurrentBatch, CancellationTokenSource.Token).ConfigureAwait(false);

                        // Returning without throwing means all of it landed.
                        CurrentBatch.Clear();
                    }
                    catch
                    {
                        // Keep whatever the write did not claim, and try it again next cycle.
                    }

                    // Partial progress counts: a sink draining slowly must not be given up on as down.
                    if (CurrentBatch.None() || CurrentBatch.Count < countBefore) FailedFlushes = 0;
                    else if (++FailedFlushes >= MaxFailedFlushesBeforeDrop)
                    {
                        // Nothing moved across every retry. Give up, so one un-writable batch cannot
                        // block every later one for ever.
                        CurrentBatch.Clear();
                        FailedFlushes = 0;
                    }
                }

                await Task.Delay(Interval, CancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        // Sends the count of dropped entries along with the next flush, so backpressure loss lands in
        // the sink as a real warning rather than vanishing.
        void AppendDropNotice()
        {
            var dropped = Interlocked.Exchange(ref DroppedCount, 0);
            if (dropped <= 0) return;

            CurrentBatch.Add(new LogMessage
            {
                Timestamp = DateTimeOffset.Now,
                Severity = (int)LogLevel.Warning,
                Message = $"[Olive logging] Dropped {dropped} log " + (dropped == 1 ? "entry" : "entries") +
                          " because the background queue was full (sink backpressure)."
            });
        }

        public void AddMessage(DateTimeOffset timestamp, string message, int severity = 0, string contextInfo = null)
        {
            if (MessageQueue.IsAddingCompleted) return;

            var item = new LogMessage
            {
                Message = message,
                ContextInfo = contextInfo,
                Timestamp = timestamp,
                Severity = severity
            };

            try
            {
                // TryAdd, not Add: logging must never block the calling thread. A full queue drops the
                // newest entry and counts it, rather than back-pressuring into request latency.
                if (!MessageQueue.TryAdd(item))
                    Interlocked.Increment(ref DroppedCount);
            }
            catch
            {
                // CompleteAdding raced with us during shutdown. Nothing to do.
            }
        }

        void Start()
        {
            // Always bounded — null, zero and negative all mean the default cap. An unbounded queue
            // turns a sink outage into an out-of-memory.
            var capacity = QueueSize is > 0 ? QueueSize.Value : DefaultBackgroundQueueSize;
            MessageQueue = new(new ConcurrentQueue<LogMessage>(), capacity);

            CancellationTokenSource = new();
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

        public ILogger CreateLogger(string categoryName) => new BatchingLogger(this, categoryName);
    }
}