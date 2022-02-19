using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive
{
    public interface IEventBusQueue
    {
        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        Task<string> Publish(string message);

        /// <summary>
        /// Publishes the specified events to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages);

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        void Subscribe(Func<string, Task> @handler);

        Task PullAll(Func<string, Task> @handler);

        /// <summary>
        /// Pulls a single item from the specified queue, or null if nothing was available.
        /// After completing the message processing, you must call Complete().
        /// </summary>
        Task<QueueMessageHandle> Pull(int timeoutSeconds = 10);

        /// <summary>
        /// Deletes all messages on the specified queue.
        /// </summary>
        Task Purge();
    }
}
