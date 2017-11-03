namespace Olive
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public interface IAsyncEvent { bool IsHandled(); }

    public abstract partial class AbstractAsyncEvent : IAsyncEvent, IDisposable
    {
        public TimeSpan? Timeout { get; set; }

        protected WeakReference<object> OwnerReference;
        protected string DeclaringFile, EventName;
        protected bool IsDisposing;

        internal protected ConcurrentList<AsyncEventHandler> Handlers = new ConcurrentList<AsyncEventHandler>();

        protected AbstractAsyncEvent(string eventName, string declaringFile)
        {
            if (Debugger.IsAttached)
            {
                EventName = eventName;
                DeclaringFile = declaringFile;
            }
        }

        public int HandlersCount => Handlers.Count;

        public void SetOwner(object owner) => OwnerReference = owner.GetWeakReference();

        public object Owner => OwnerReference.GetTargetOrDefault();

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasHandler(object handlerFunction)
        {
            return Handlers.Any(x => ReferenceEquals(((IAsyncEventHandler)x).Action, handlerFunction));
        }

        public bool IsHandled() => Handlers.Any();

        protected string DeclaringType => DeclaringFile.OrEmpty().Split('\\').LastOrDefault().TrimEnd(".cs", caseSensitive: false).Split('.').FirstOrDefault();

        public override string ToString() => DeclaringType + "." + EventName + " ◀ " + Owner;

        internal void RemoveHandler<TActionFunction>(AsyncEventHandler handler)
        {
            lock (Handlers) Handlers.Remove(handler);
        }

        /// <summary>Removes all current handlers from this event.</summary>
        public void ClearHandlers() => Handlers = new ConcurrentList<AsyncEventHandler>();

        /// <summary>
        /// Returns a tasks that completes once as soon as this event is fired.
        /// </summary>
        public Task AwaitRaiseCompletion()
        {
            var completionTask = new TaskCompletionSource<bool>();

            void waiter()
            {
                completionTask.TrySetResult(result: true);
                this.RemoveActionHandler(waiter);
            }

            this.HandleWith(waiter);
            return completionTask.Task;
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TReturn DoHandleOn<TArg, TReturn>(Func<TArg, Task> handlerTask,
            Action<TArg> handlerAction, string callerFile, int line)
            where TReturn : AbstractAsyncEvent
        {
            if (handlerTask == null && handlerAction == null) return (TReturn)this;

            var caller = Debugger.IsAttached ? $"{callerFile}:{line}" : string.Empty;

            if (handlerTask != null && !HasHandler(handlerTask))
                Handlers.Add(new AsyncEventTaskHandler<TArg>
                {
                    Action = handlerTask,
                    Event = this,
                    Caller = caller
                });

            if (handlerAction != null && !HasHandler(handlerTask))
                Handlers.Add(new AsyncEventActionHandler<TArg>
                {
                    Action = handlerAction,
                    Event = this,
                    Caller = caller
                });

            return (TReturn)this;
        }

        public void Dispose()
        {
            IsDisposing = true;
            ClearHandlers();
        }
    }
}