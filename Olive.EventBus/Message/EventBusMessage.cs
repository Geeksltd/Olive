using System;
using System.Threading.Tasks;

namespace Olive
{
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
}
