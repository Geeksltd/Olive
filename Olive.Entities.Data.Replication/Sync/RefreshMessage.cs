using System;

namespace Olive.Entities.Replication
{
    class RefreshMessage : IEventBusMessage
    {
        public string TypeName { get; set; }

        public string DeduplicationId => TypeName;
        public string MessageGroupId => "Default";

        public DateTime RequestUtc { get; set; }
    }
}