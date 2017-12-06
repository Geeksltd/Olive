namespace Olive
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public interface IAsyncEventHandler : IDisposable { object Action { get; } }

    /// <summary>
    /// Provides a mechanism to prevent event handler dependency memory leaks.
    /// 
    /// </summary>
    public class EventHandlerDisposer
    {
        object SyncLock = new object();
        List<WeakReference<IAsyncEventHandler>> Dependencies = new List<WeakReference<IAsyncEventHandler>>();

        /// <summary>
        /// Will dispose all registered event handlers and clear them from the list.
        /// </summary>
        public void DisposeAll()
        {
            lock (SyncLock)
            {
                foreach (var c in Dependencies.ToArray())
                {
                    c.GetTargetOrDefault()?.Dispose();
                    c.SetTarget(null);
                    Dependencies.Remove(c);
                }
            }
        }

        public void Register(IAsyncEventHandler handler)
        {
            if (handler == null) return;

            lock (SyncLock)
                Dependencies.Add(handler.GetWeakReference());
        }
    }

    public abstract class AsyncEventHandler : IDisposable, IEquatable<AsyncEventHandler>
    {
        internal string Caller;
        protected bool IsDisposed;

        [DebuggerStepThrough]
        protected Task Raise(Func<Task> raiser)
        {
            if (IsDisposed) return Task.CompletedTask;

            if (raiser == null) return Task.CompletedTask;
            return raiser.Invoke();
        }

        [DebuggerStepThrough]
        protected Task Raise(Action action)
        {
            if (IsDisposed) return Task.CompletedTask;

            action?.Invoke();

            return Task.CompletedTask;
        }

        public abstract bool Equals(AsyncEventHandler other);

        public abstract void Dispose();
    }

    public abstract class AsyncEventHandler<TActionFunction> : AsyncEventHandler, IAsyncEventHandler
        where TActionFunction : class
    {
        internal TActionFunction Action;
        // WeakReference<AbstractAsyncEvent> EventRef;

        internal AbstractAsyncEvent Event { get; set; }

        // internal AbstractAsyncEvent Event
        // {
        //    get => EventRef?.GetTargetOrDefault();
        //    set => value?.GetWeakReference();
        // }

        object IAsyncEventHandler.Action => Action;

        public override bool Equals(AsyncEventHandler other)
        {
            return Action == (other as AsyncEventHandler<TActionFunction>)?.Action;
        }

        public override void Dispose()
        {
            IsDisposed = true;

            Event?.RemoveHandler<AsyncEventHandler>(this);
            // EventRef = null;
            Event = null;
            Action = null;
            Caller = null;
        }
    }

    public class AsyncEventActionHandler : AsyncEventHandler<Action>
    {
        [DebuggerStepThrough]
        internal Task Raise() => Raise(Action);
    }

    public class AsyncEventTaskHandler : AsyncEventHandler<Func<Task>>
    {
        [DebuggerStepThrough]
        internal Task Raise() => Raise(RaiseIt);

        [DebuggerStepThrough]
        Task RaiseIt() => Action?.Invoke() ?? Task.CompletedTask;
    }

    public class AsyncEventActionHandler<T> : AsyncEventHandler<Action<T>>
    {
        [DebuggerStepThrough]
        internal Task Raise(T arg) => Raise(() => Action?.Invoke(arg));
    }

    public class AsyncEventTaskHandler<T> : AsyncEventHandler<Func<T, Task>>
    {
        [DebuggerStepThrough]
        internal Task Raise(T arg) => Raise(() => Action?.Invoke(arg) ?? Task.CompletedTask);
    }
}