using System;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// An event bus queue that handles specific types of messages.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class EventBusQueue<TMessage> : IEventBusQueue where TMessage : IEventBusMessage
    {
        public IEventBusQueue Queue { get; }

        public EventBusQueue(IEventBusQueue queue) => Queue = queue;

        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public Task<string> Publish(IEventBusMessage message) => Queue.Publish(message);

        void IEventBusQueue.Subscribe<TMsg>(Func<TMsg, Task> @handler) => Queue.Subscribe(handler);

        /// <summary>
        /// Attaches an event handler to this queue's message type.
        /// </summary>
        public void Subscribe(Func<TMessage, Task> @handler) => Queue.Subscribe(handler);

        /// <summary>
        /// Deletes all messages on the specified queue.
        /// </summary>
        public Task Purge() => Queue.Purge();
    }
}
