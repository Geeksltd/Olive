namespace Olive
{
    public class AsyncEvent : AbstractAsyncEvent
    {
        public AsyncEvent([CallerMemberName] string eventName = "", [CallerFilePath] string declaringFile = "") : base(eventName, declaringFile) { }

        public AsyncEvent(ConcurrentEventRaisePolicy raisePolicy, [CallerMemberName] string eventName = "", [CallerFilePath] string declaringFile = "") : base(eventName, declaringFile)
        {
            ConcurrentRaisePolicy = raisePolicy;
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task RaiseHandler(AsyncEventHandler handler)
        {
            if (handler is AsyncEventActionHandler h) return h.Raise();
            else if (handler is AsyncEventTaskHandler t) return t.Raise();
            else return Task.CompletedTask;
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Raise() => Raise(inParallel: false);

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Raise(bool inParallel) => Raise(RaiseHandler, inParallel);
    }
}