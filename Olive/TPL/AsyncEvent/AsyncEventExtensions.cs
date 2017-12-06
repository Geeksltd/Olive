namespace Olive
{
    public static class AsyncEventExtensions
    {
        /// <summary>
        /// The same as RemoveHandler.
        /// It's added to get past the strange bug in C# for selecting the correct overload of RemoveHandler().
        /// </summary>
        public static TEvent RemoveActionHandler<TEvent>(this TEvent @event, Action handler)
            where TEvent : AbstractAsyncEvent
        {
            return @event.DoRemoveHandler(handler);
        }

        [DebuggerStepThrough]
        public static TEvent RemoveActionHandler<TEvent, TArg>(this TEvent @event, Action<TArg> handler)
            where TEvent : AbstractAsyncEvent
        {
            return @event.DoRemoveHandler(handler);
        }

        [DebuggerStepThrough]
        public static TEvent RemoveActionHandler<TEvent, TArg1, TArg2>(this TEvent @event, Action<TArg1, TArg2> handler)
            where TEvent : AbstractAsyncEvent
        {
            return @event.DoRemoveHandler(handler);
        }

        [DebuggerStepThrough]
        public static TEvent RemoveHandler<TEvent>(this TEvent @event, Func<Task> handler)
            where TEvent : AbstractAsyncEvent
        {
            return @event.DoRemoveHandler(handler);
        }

        [DebuggerStepThrough]
        public static TEvent RemoveHandler<TEvent, TArg>(this TEvent @event, Func<TArg, Task> handler)
            where TEvent : AbstractAsyncEvent
        {
            return @event.DoRemoveHandler(handler);
        }

        [DebuggerStepThrough]
        public static TEvent RemoveHandler<TEvent, TArg1, TArg2>(this TEvent @event, Func<TArg1, TArg2, Task> handler)
           where TEvent : AbstractAsyncEvent
        {
            return @event.DoRemoveHandler(handler);
        }

        [DebuggerStepThrough]
        internal static TEvent DoRemoveHandler<TEvent>(this TEvent @event, object handlerFunction)
            where TEvent : AbstractAsyncEvent
        {
            lock (@event.Handlers)
            {
                var itemsToRemove = @event.Handlers.Where(x => ((IAsyncEventHandler)x).Action == handlerFunction).ToArray();
                itemsToRemove.Do(x => x.Dispose());
                @event.Handlers.Remove(itemsToRemove);
            }

            return @event;
        }

        /// <summary>
        /// The same as Handle. It's added to get past the strange bug in C# for selecting the correct overload of Handle().
        /// </summary> 
        public static TEvent HandleWith<TEvent>(this TEvent @event, Action handler,
           [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0)
            where TEvent : AbstractAsyncEvent
        {
            return Handle(@event, handler, callerFile, callerLine);
        }

        public static TEvent Handle<TEvent>(this TEvent @event, Action handler,
           [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0)
            where TEvent : AbstractAsyncEvent
        {
            return HandleOn(@event, handler, callerFile, callerLine);
        }

        public static TEvent Handle<TEvent>(this TEvent @event, Func<Task> handler,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0)
            where TEvent : AbstractAsyncEvent
        {
            return HandleOn(@event, handler, callerFile, callerLine);
        }

        /// <summary>
        /// The same as HandleOn. It's added to get past the strange bug in C# for selecting the correct overload of HandleOn().
        /// </summary> 
        public static TEvent HandleActionOn<TEvent>(this TEvent @event, Action handler,
             [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
            where TEvent : AbstractAsyncEvent
        {
            return HandleOn(@event, handler, callerFile, line);
        }

        public static TEvent HandleOn<TEvent>(this TEvent @event, Action handler,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
            where TEvent : AbstractAsyncEvent
        {
            if (handler == null) return @event;

            lock (@event.Handlers)
                @event.Handlers.AddUnique(new AsyncEventActionHandler
                {
                    Action = handler,
                    Event = @event,
                    Caller = Debugger.IsAttached ? $"{callerFile}:{line}" : string.Empty
                });

            return @event;
        }

        /// <summary>
        /// Creates an event handler which you can dispose of explicitly if required.
        /// </summary>
        public static IAsyncEventHandler CreateActionHandler<TEvent>(this TEvent @event, Action handler,
           [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
           where TEvent : AbstractAsyncEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (@event.Handlers)
            {
                @event.RemoveActionHandler(handler);

                var result = new AsyncEventActionHandler
                {
                    Action = handler,
                    Event = @event,
                    Caller = Debugger.IsAttached ? $"{callerFile}:{line}" : string.Empty
                };

                @event.Handlers.Add(result);
                return result;
            }
        }

        /// <summary>
        /// Creates an event handler which you can dispose of explicitly if required.
        /// </summary>
        public static IAsyncEventHandler CreateHandler<TEvent>(this TEvent @event, Func<Task> handler,
           [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
           where TEvent : AbstractAsyncEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (@event.Handlers)
            {
                @event.RemoveHandler(handler);

                var result = new AsyncEventTaskHandler
                {
                    Action = handler,
                    Event = @event,
                    Caller = Debugger.IsAttached ? $"{callerFile}:{line}" : string.Empty
                };

                @event.Handlers.Add(result);
                return result;
            }
        }

        public static TEvent HandleOn<TEvent>(this TEvent @event, Func<Task> handler,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int line = 0)
            where TEvent : AbstractAsyncEvent
        {
            if (handler == null) return @event;

            lock (@event.Handlers)
            {
                @event.Handlers.AddUnique(new AsyncEventTaskHandler
                {
                    Action = handler,
                    Event = @event,
                    Caller = Debugger.IsAttached ? $"{callerFile}:{line}" : string.Empty
                });
            }

            return @event;
        }
    }
}