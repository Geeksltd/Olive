using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public class EndpointSubscriber
    {
        static FieldInfo IsImmutableField;
        static PropertyInfo IsNewProperty;
        public Type DomainType { get; set; }
        public DestinationEndpoint Endpoint { get; }
        DateTime? RefreshRequestUtc;
        ILogger Log;

        static EndpointSubscriber()
        {
            IsImmutableField = typeof(Entity).GetField("IsImmutable", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Field IsImmutable was not found on type Entity.");

            IsNewProperty = typeof(Entity).GetProperty("IsNew")
                ?? throw new Exception("Property IsNew was not found on type Entity.");
        }

        public EndpointSubscriber(DestinationEndpoint endpoint, Type domainType)
        {
            Endpoint = endpoint;
            DomainType = domainType;
            Log = Olive.Log.For(this);
        }

        protected IDatabase Database => Context.Current.Database();

        public async Task RefreshData()
        {
            Log.Warning("Data table " + DomainType.Name + " is empty. Adding a refresh message.");

            var request = new RefreshMessage { TypeName = DomainType.Namespace + "." + DomainType.Name, RequestUtc = DateTime.UtcNow };
            RefreshRequestUtc = request.RequestUtc;
            await Endpoint.RefreshQueue.Publish(request);

            Log.Warning("Refresh message published to queue.");

            await Database.Refresh();
        }

        internal async Task Import(ReplicateDataMessage message)
        {
            if (message.CreationUtc < RefreshRequestUtc)
            {
                // Ignore this. We will receive a full table after this anyway.
                Log.Info("Ignoring importing expired ReplicateDataMessage " + message.DeduplicationId);
                return;
            }

            Log.Debug($"Beginning to import ReplicateDataMessage for {message.TypeFullName}:\n{message.Entity}\n\n");
            
            IEntity entity;

            try { entity = await Deserialize(message.Entity); }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to deserialize.");
                throw;
            }

            try
            {
                var mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;
                await Database.Save(entity, SaveBehaviour.BypassAll);
                await GlobalEntityEvents.InstanceSaved.Raise(new GlobalSaveEventArgs(entity, mode));

                Log.Debug("Saved the " + entity.GetType().FullName + " " + entity.GetId());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to import.");
                throw;
            }
        }

        async Task<IEntity> Deserialize(string serialized)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialized);

            var result = (await Database.GetOrDefault(data["ID"], DomainType))?.Clone();
            if (result == null) result = DomainType.CreateInstance<IEntity>();

            foreach (var field in data)
            {
                var property = DomainType.GetProperty(field.Key);
                if (property.PropertyType.IsA<IEntity>())
                    property = property.DeclaringType.GetProperty(property.Name + "Id");

                property.SetValue(result, field.Value.To(property.PropertyType));
            }

            return result;
        }
    }
}