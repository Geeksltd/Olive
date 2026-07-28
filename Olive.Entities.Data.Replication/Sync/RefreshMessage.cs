using System;

namespace Olive.Entities.Replication
{
    /// <summary>
    /// Derives from EventBusMessage, rather than implementing IEventBusMessage directly, so that it can
    /// carry a ReferenceCode like every other message.
    /// </summary>
    class RefreshMessage : EventBusMessage
    {
        /// <summary>Also the deduplication id, as it has always been: one pending refresh per type.</summary>
        public string TypeName { get => DeduplicationId; set => DeduplicationId = value; }

        public DateTime RequestUtc { get; set; }
    }
}
