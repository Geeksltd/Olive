using System;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// Represents a message sent or received using a queue-based communication.
    /// </summary>
    public interface IEventBusMessage
    {
        /// <summary>
        /// The ID used by queue systems to deduplicate a message (in cases of double delivery).
        /// </summary>
        string DeduplicationId { get; }
    }

    /// <summary>
    /// A basic implementation of IEventBusMessage with a new Guid used as DeduplicationId.
    /// </summary>
    public abstract class EventBusMessage : IEventBusMessage
    {
        /// <summary>
        /// By default it's a new Guid.
        /// </summary>
        public string DeduplicationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// A self-fulfilling event bus message.
    /// </summary>
    public abstract class EventBusCommandMessage : EventBusMessage
    {
        /// <summary>
        /// Will handle the received message. It should not hide exceptions.
        /// </summary>
        public abstract Task Process();
    }
}
