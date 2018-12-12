using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract partial class SourceEndpoint
    {
        Dictionary<string, ReplicatedData> Agents = new Dictionary<string, ReplicatedData>();

        string UrlPattern => Config.GetOrThrow("DataReplication:" + GetType().FullName + ":Url");

        protected virtual IEnumerable<Type> GetTypes()
        {
            return GetType().GetCustomAttributes<ExportDataAttribute>()
                .Select(x => x.Type)
                .Concat(GetType().GetNestedTypes().Where(x => x.IsA<ReplicatedData>()))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Starts publishing an end point for the specified data types. 
        /// It handles all save events on such objects, and publishes them on the event bus.
        /// </summary>
        public void Publish()
        {
            var types = GetTypes();

            if (types.None())
                throw new Exception("No data is exported on " + GetType().FullName);

            foreach (var type in types)
            {
                var agent = type.CreateInstance<ReplicatedData>();
                agent.Define();

                agent.QueueUrl = UrlPattern;
                Agents.Add(type.FullName, agent);
                agent.Start();
            }

            HandleRefreshRequests();
        }

        void HandleRefreshRequests()
        {
            EventBus.Queue(UrlPattern.TrimEnd(".fifo") + "-REFRESH.fifo").Subscribe<RefreshMessage>(message =>
            {
                Agents[message.TypeName].UploadAll();
                return Task.CompletedTask;
            });
        }
    }
}