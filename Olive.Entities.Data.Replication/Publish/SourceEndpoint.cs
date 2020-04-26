using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract partial class SourceEndpoint
    {
        string RefreshQueueUrl => UrlPattern.TrimEnd(".fifo") + "-REFRESH.fifo";
        Dictionary<string, ExposedType> Agents = new Dictionary<string, ExposedType>();
        public IEnumerable<string> ExposedTypes => Agents.Keys;

        string UrlPattern => Config.GetOrThrow("DataReplication:" + GetType().FullName + ":Url");

        public virtual IEnumerable<Type> GetTypes()
        {
            return GetType().GetCustomAttributes<ExportDataAttribute>()
                .Select(x => x.Type)
                .Concat(GetType().GetNestedTypes(BindingFlags.NonPublic).Where(x => x.IsA<ExposedType>()))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Starts publishing an end point for the specified data types. 
        /// It handles all save events on such objects, and publishes them on the event bus.
        /// </summary>
        public void Publish(bool handleRefreshRequests = true)
        {
            var types = GetTypes();

            if (types.None())
                throw new Exception("No data is exported on " + GetType().FullName);

            foreach (var type in types)
            {
                var agent = type.CreateInstance<ExposedType>();
                agent.Define();

                agent.QueueUrl = UrlPattern;
                Agents.Add(type.Namespace + "." + type.Name, agent);
                agent.Start();
            }

            if (handleRefreshRequests)
                HandleRefreshRequests();
        }

        async Task HandleRefreshMessage(string typeName)
        {
            if (Agents.TryGetValue(typeName, out var agent))
            {
                Log.For(this).Debug("Uploading all data for " + typeName);
                await agent.UploadAll();
                Log.For(this).Debug("Finished uploading all data for " + typeName);
            }
            else
            {
                var exception = new Exception("There is no published endpoint for the type: " + typeName +
                    "\r\n\r\nRegistered types are:\r\n" +
                    Agents.Select(x => x.Key).ToLinesString());
                Log.For(this).Error(exception, "Failed to UploadAll");
                throw exception;
            }
        }

        public Task UploadAll(string typeName) => HandleRefreshMessage(typeName);
        public Task UploadAll() => Agents.Do(i => HandleRefreshMessage(i.Key));

        void HandleRefreshRequests()
        {
            Log.For(this).Debug("Subscribing to " + RefreshQueueUrl);
            EventBus.Queue(RefreshQueueUrl).Subscribe<RefreshMessage>(async message => await HandleRefreshMessage(message.TypeName));
        }
    }
}