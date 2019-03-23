using System;

namespace Olive.Entities.Replication
{
    public class ReplicateDataMessage : EventBusMessage
    {
        public string TypeFullName { get; set; }
        public string Entity { get; set; }
        public DateTime CreationUtc { get; set; }
        public bool ToDelete { get; set; }
    }
}
