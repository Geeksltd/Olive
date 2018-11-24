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
        public static Task<string> Publish<TMessage>(TMessage message)
            where TMessage : class, IEventBusMessage
            => Provider.Publish(message ?? throw new ArgumentNullException(nameof(message)));

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        public static void Subscribe<TMessage>(Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
            => Provider.Subscribe(handler ?? throw new ArgumentNullException(nameof(handler)));
    }
}
