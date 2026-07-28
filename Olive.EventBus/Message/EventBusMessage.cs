using System;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// A basic implementation of IEventBusMessage with a new Guid used as DeduplicationId and 'Default' as group.
    /// </summary>
    public abstract class EventBusMessage : IEventBusMessage
    {
        /// <summary>
        /// By default it's a new Guid.
        /// </summary>
        public string DeduplicationId { get; set; } = Guid.NewGuid().ToString().Remove("-");

        public string MessageGroupId { get; set; } = "Default";

        /// <summary>
        /// The reference code of the work that published this message, stamped on publish and adopted
        /// by the handler. Null when it was published outside any known unit of work.
        /// </summary>
        public string ReferenceCode { get; set; }
    }
}
