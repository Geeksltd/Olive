using System;
using System.Threading.Tasks;

namespace Olive
{
    public interface IEventBusQueue
    {
        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        Task<string> Publish(IEventBusMessage message);

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        void Subscribe<TMessage>(Func<TMessage, Task> @handler) where TMessage : IEventBusMessage;

        /// <summary>
        /// Pulls a single item from the specified queue, or null if nothing was available.
        /// After completing the message processing, you must call Complete().
        /// </summary>
        Task<QueueMessageHandle<TMessage>> Pull<TMessage>(int timeoutSeconds = 10)
           where TMessage : IEventBusMessage;

        /// <summary>
        /// Deletes all messages on the specified queue.
        /// </summary>
        Task Purge();
    }
}
