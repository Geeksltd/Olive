using Newtonsoft.Json;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public class DataReplication
    {
        public static void Subscribe<TDomain>()
        {
            // fetch from the queue and insert into the local table.
        }
    }
}
