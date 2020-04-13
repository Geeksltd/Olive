namespace Olive
{
    using System;
    using System.Threading.Tasks;

    partial class OliveExtensions
    {
        public static async Task Raise<TArgs>(this AwaitableEventHandler<TArgs> @event, TArgs args)
        {
            var handlers = @event?.GetInvocationList();
            if (handlers is null) return;

            foreach (var handler in handlers)
            {
                var typedHandler = (AwaitableEventHandler<TArgs>)handler;
                var ev = new AwaitableEvent<TArgs>(args);

                typedHandler(ev);

                if (ev.AsyncHandler != null)
                {
                    await ev.AsyncHandler.Invoke(args);
                    ev.AsyncHandler = null; // Help GC
                }
            }
        }

        public static async Task Raise(this AwaitableEventHandler @event)
        {
            var handlers = @event?.GetInvocationList();
            if (handlers is null) return;

            foreach (var handler in handlers)
            {
                var typedHandler = (AwaitableEventHandler)handler;
                var ev = new AwaitableEvent();

                typedHandler(ev);

                if (ev.AsyncHandler != null)
                {
                    await ev.AsyncHandler.Invoke();
                    ev.AsyncHandler = null; // Help GC
                }
            }
        }
    }
}