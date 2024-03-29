﻿using System;

namespace Olive.Entities.EF.Replication
{
    class RefreshMessage : IEventBusMessage
    {
        public string TypeName { get; set; }

        public string DeduplicationId => TypeName;

        public DateTime RequestUtc { get; set; }
    }
}