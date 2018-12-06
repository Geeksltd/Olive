using System;

namespace Olive
{
    public interface IEventBusMessage
    {
        string DeduplicationId { get; }
    }

    public abstract class EventBusMessage : IEventBusMessage
    {
        /// <summary>
        /// By default it's a new Guid.
        /// </summary>
        public string DeduplicationId { get; set; } = Guid.NewGuid().ToString();
    }
}
