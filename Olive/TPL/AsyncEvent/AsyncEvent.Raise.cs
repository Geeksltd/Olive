namespace Olive
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    partial class AbstractAsyncEvent
    {
        AsyncLock RaisingLock;
        bool IsRaising;

        /// <summary>
        /// Determines how concurrent attempts to raise an event should be handled.
        /// </summary>
        protected ConcurrentEventRaisePolicy ConcurrentRaisePolicy = ConcurrentEventRaisePolicy.Parallel;

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async Task Raise(Func<AsyncEventHandler, Task> raiser, bool inParallel)
        {
            if (Handlers.None()) return;

            var handlers = Handlers.ToArray();

            await Raise(handlers, raiser, inParallel);
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task Raise(AsyncEventHandler[] handlers, Func<AsyncEventHandler, Task> raiser, bool inParallel)
        {
            switch (ConcurrentRaisePolicy)
            {
                case ConcurrentEventRaisePolicy.Parallel: return RaiseOnce(handlers, raiser, inParallel);
                case ConcurrentEventRaisePolicy.Ignore: return RaiseWithIgnorePolicy(handlers, raiser, inParallel);
                case ConcurrentEventRaisePolicy.Queue: return RaiseWithQueuePolicy(handlers, raiser, inParallel);
                default: throw new NotImplementedException();
            }
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task RaiseWithIgnorePolicy(AsyncEventHandler[] handlers, Func<AsyncEventHandler, Task> raiser, bool inParallel)
        {
            if (IsRaising) return;

            IsRaising = true;
            try { await RaiseOnce(handlers, raiser, inParallel); }
            finally { IsRaising = false; }
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task RaiseWithQueuePolicy(AsyncEventHandler[] handlers, Func<AsyncEventHandler, Task> raiser, bool inParallel)
        {
            if (RaisingLock == null) RaisingLock = new AsyncLock();

            using (await RaisingLock.Lock()) await RaiseOnce(handlers, raiser, inParallel);
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task RaiseOnce(AsyncEventHandler[] handlers, Func<AsyncEventHandler, Task> raiser, bool inParallel)
        {
            try
            {
                if (inParallel)
                {
                    await Task.WhenAll(handlers.Select(x =>
                    {
                        if (!IsDisposing) return raiser(x);
                        else return Task.CompletedTask;
                    }));
                }
                else foreach (var h in handlers)
                        if (!IsDisposing) await raiser(h);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raising the event {DeclaringType}.{EventName} failed: " + ex.ToLogString());
                throw;
            }
        }
    }
}
