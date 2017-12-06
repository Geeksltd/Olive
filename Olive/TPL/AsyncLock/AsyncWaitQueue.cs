namespace Olive
{
    using System;
    using System.Threading.Tasks;

    internal sealed class AsyncWaitQueue<T>
    {
        readonly AsyncLockDeque<TaskCompletionSource<T>> Queue = new AsyncLockDeque<TaskCompletionSource<T>>();

        public int Count { get { lock (Queue) return Queue.Count; } }

        public Task<T> Enqueue()
        {
            var source = new TaskCompletionSource<T>();
            lock (Queue) Queue.Enqueue(source);
            return source.Task;
        }

        public IDisposable Dequeue(T result)
        {
            lock (Queue) return new CompleteDisposable(result, Queue.Deque());
        }

        sealed class CompleteDisposable : IDisposable
        {
            readonly TaskCompletionSource<T>[] Sources;
            readonly T Result;

            public CompleteDisposable(T result, params TaskCompletionSource<T>[] taskCompletionSources)
            {
                Result = result;
                Sources = taskCompletionSources;
            }

            public void Dispose()
            {
                foreach (var s in Sources)
                {
                    Task.Run(() => s.TrySetResult(Result));
                    s.Task.Wait();
                }
            }
        }
    }
}
