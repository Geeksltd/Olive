using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public class ReplicateDataMessage : EventBusMessage
    {
        public string Entity { get; set; }
    }
}
