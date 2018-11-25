using System;
using System.Threading.Tasks;

namespace Olive
{
    public class EventBus
    {
        static IEventBusProvider Provider => Context.Current.GetService<IEventBusProvider>();

        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public static Task<string> Publish(string queueKey, IEventBusMessage message)
            => Provider.Publish(queueKey, message ?? throw new ArgumentNullException(nameof(message)));

        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public static Task<string> Publish<TMessage>(TMessage message)
            where TMessage : class, IEventBusMessage
            => Publish(typeof(TMessage).FullName, message ?? throw new ArgumentNullException(nameof(message)));

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        public static void Subscribe<TMessage>(Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
            => Subscribe(typeof(TMessage).FullName, handler ?? throw new ArgumentNullException(nameof(handler)));

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        public static void Subscribe<TMessage>(string queueKey, Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
            => Provider.Subscribe(queueKey, handler ?? throw new ArgumentNullException(nameof(handler)));
    }
}
