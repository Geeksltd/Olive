namespace Olive
{
    using System;
    using System.Threading.Tasks;

    /// <summary> A recursive mutual exclusion lock that to use with async code.</summary>
    public sealed class AsyncLock
    {
        bool IsTaken;
        readonly AsyncWaitQueue<IDisposable> Queue = new AsyncWaitQueue<IDisposable>();
        readonly Task<IDisposable> WaitTask;
        readonly object Mutex = new object();

        public AsyncLock()
        {
            WaitTask = Task.FromResult<IDisposable>(new Key(this));
        }

        public DisposableAwaitable<IDisposable> Lock()
        {
            lock (Mutex)
            {
                if (IsTaken) return new DisposableAwaitable<IDisposable>(Queue.Enqueue());
                else
                {
                    IsTaken = true;
                    return new DisposableAwaitable<IDisposable>(WaitTask);
                }
            }
        }

        void Release()
        {
            IDisposable toRelease = null;

            lock (Mutex)
            {
                if (Queue.Count == 0) IsTaken = false;
                else toRelease = Queue.Dequeue(WaitTask.Result);
            }

            toRelease?.Dispose();
        }

        sealed class Key : IDisposable
        {
            readonly AsyncLock Lock;

            public Key(AsyncLock asyncLock) { Lock = asyncLock; }

            public void Dispose() => Lock.Release();
        }
    }
}
