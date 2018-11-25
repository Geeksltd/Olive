using System;
using System.Linq;
using System.Reflection;

namespace Olive.Entities
{
    public class DataReplicationEndPoint
    {
        /// <summary>
        /// Starts publishing an end point for the specified data types. 
        /// It handles all save events on such objects, and publishes them on the event bus.
        /// </summary>
        public void Publish()
        {
            var types = GetType().GetCustomAttributes<ExportDataAttribute>().Select(x => x.Type).Distinct();

            if (types.None())
                throw new Exception("No data is exported on " + GetType().FullName);

            types.Select(x => x.CreateInstance<ReplicatedData>()).Do(x => x.Start());
        }
    }
}