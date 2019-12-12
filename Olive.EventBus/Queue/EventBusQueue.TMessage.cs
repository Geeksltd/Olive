using System;
using System.Collections.Generic;
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

        Task<string> IEventBusQueue.Publish(string message) => Queue.Publish(message);
        Task<IEnumerable<string>> IEventBusQueue.PublishBatch(IEnumerable<string> messages)
            => Queue.PublishBatch(messages);
        void IEventBusQueue.Subscribe(Func<string, Task> @handler) => Queue.Subscribe(handler);
        Task IEventBusQueue.PullAll(Func<string, Task> @handler) => Queue.PullAll(handler);
        Task<QueueMessageHandle> IEventBusQueue.Pull(int timeout) => Queue.Pull(timeout);
        Task IEventBusQueue.Purge() => Queue.Purge();

        /// <summary>
        /// Pulls a single item from the specified queue, or null if nothing was available.
        /// After completing the message processing, you must call Complete().
        /// </summary>
        public Task<QueueMessageHandle<TMessage>> Pull<TMsg>(int timeoutSeconds = 10)
            where TMsg : IEventBusMessage => Queue.Pull<TMessage>(timeoutSeconds);


        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public Task<string> Publish(TMessage message)
        {
            return this.Publish((IEventBusMessage)message);
        }


        /// <summary>
        /// Publishes the specified events to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public Task<IEnumerable<string>> PublishBatch(IEnumerable<TMessage> messages)
        {
            return this.PublishBatch((dynamic)messages as IEnumerable<IEventBusMessage>);
        }

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        public void Subscribe(Func<TMessage, Task> @handler)
        {
            ((IEventBusQueue)this).Subscribe(handler);
        }

        public Task PullAll(Func<TMessage, Task> @handler) => ((IEventBusQueue)this).PullAll(handler);

        /// <summary>
        /// Pulls a single item from the specified queue, or null if nothing was available.
        /// After completing the message processing, you must call Complete().
        /// </summary>
        public Task<QueueMessageHandle<TMessage>> Pull(int timeoutSeconds = 10)
        {
            return ((IEventBusQueue)this).Pull<TMessage>(timeoutSeconds);
        }
    }
}
