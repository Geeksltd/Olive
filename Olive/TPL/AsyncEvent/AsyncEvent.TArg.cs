namespace Olive
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class AsyncEvent<TArg> : AbstractAsyncEvent
    {
        public AsyncEvent([CallerMemberName] string eventName = "", [CallerFilePath] string declaringFile = "")
            : base(eventName, declaringFile) { }

        public AsyncEvent(ConcurrentEventRaisePolicy raisePolicy, [CallerMemberName] string eventName = "", [CallerFilePath] string declaringFile = "") : base(eventName, declaringFile)
        {
            ConcurrentRaisePolicy = raisePolicy;
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncEvent<TArg> Handle(Func<TArg, Task> handler,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
        {
            return DoHandleOn<TArg, AsyncEvent<TArg>>(handler, null, callerFile, line);
        }

        /// <summary>
        /// The same as Handle. It's added to get past the strange bug in C# for selecting the correct overload of Handle().
        /// </summary> 
        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncEvent<TArg> HandleWith(Action<TArg> handler,
           [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
        {
            return DoHandleOn(null, handler, callerFile, line);
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncEvent<TArg> Handle(Action<TArg> handler,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
        {
            return DoHandleOn(null, handler, callerFile, line);
        }

        protected AsyncEvent<TArg> DoHandleOn(Func<TArg, Task> handlerTask, Action<TArg> handlerAction,
            string callerFile, int line)
        {
            return DoHandleOn<TArg, AsyncEvent<TArg>>(handlerTask, handlerAction, callerFile, line);
        }

        [DebuggerStepThrough]
        public IAsyncEventHandler CreateHandler(Func<TArg, Task> handler,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (Handlers)
            {
                RemoveHandler(handler);

                var result = new AsyncEventTaskHandler<TArg>
                {
                    Action = handler,
                    Event = this,
                    Caller = Debugger.IsAttached ? $"{callerFile}:{line}" : string.Empty
                };

                Handlers.Add(result);
                return result;
            }
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task RaiseHandler(AsyncEventHandler handler, TArg arg)
        {
            if (handler is AsyncEventActionHandler h) return h.Raise();
            else if (handler is AsyncEventTaskHandler t) return t.Raise();
            else if (handler is AsyncEventActionHandler<TArg> ha) return ha.Raise(arg);
            else if (handler is AsyncEventTaskHandler<TArg> aa) return aa.Raise(arg);
            else return Task.CompletedTask;
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Raise(TArg arg) => Raise(arg, inParallel: false);

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Raise(TArg arg, bool inParallel) => Raise(h => RaiseHandler(h, arg), inParallel);

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncEvent<TArg> RemoveHandler(Action<TArg> handler) => this.DoRemoveHandler(handler);

        /// <summary>
        /// The same as RemoveHandler.
        /// It's added to get past the strange bug in C# for selecting the correct overload of RemoveHandler().
        /// </summary>
        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncEvent<TArg> RemoveActionHandler(Action<TArg> handler) => this.DoRemoveHandler(handler);

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncEvent<TArg> RemoveHandler(Func<TArg, Task> handler) => this.DoRemoveHandler(handler);
    }
}