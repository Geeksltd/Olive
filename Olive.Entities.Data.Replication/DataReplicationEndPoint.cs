using System;
using System.Linq;
using System.Reflection;

namespace Olive.Entities
{
    public abstract class DataReplicationEndPoint
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

            foreach (var type in types)
            {
                var agent = type.CreateInstance<ReplicatedData>();
                agent.DefaultQueueConfigKey = QueueConfigKey;
                agent.Start();
            }
        }

        protected abstract string QueueConfigKey { get; }
    }
}