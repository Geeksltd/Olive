using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public class EndpointSubscriber
    {
        public Type DomainType { get; set; }
        public DestinationEndpoint Endpoint { get; }
        DateTime? RefreshRequestUtc;
        ILogger Log;

        public EndpointSubscriber(DestinationEndpoint endpoint, Type domainType)
        {
            Endpoint = endpoint;
            DomainType = domainType;
            Log = Olive.Log.For(this);
        }

        protected IDatabase Database => Context.Current.Database();

        public async Task RefreshData()
        {
            var request = new RefreshMessage { TypeName = DomainType.Namespace + "." + DomainType.Name, RequestUtc = DateTime.UtcNow };
            RefreshRequestUtc = request.RequestUtc;
            await Endpoint.RefreshQueue.Publish(request);
            await Database.Refresh();
        }

        internal async Task Import(ReplicateDataMessage message)
        {
            if (message.CreationUtc < RefreshRequestUtc)
            {
                // Ignore this. We will receive a full table after this anyway.
                Log.Info("Ignoring expired ReplicateDataMessage " + message.DeduplicationId);
                return;
            }

            Log.Debug($"Beginning to import ReplicateDataMessage for {message.TypeFullName}:\n{message.Entity}\n\n");

            IEntity entity;

            try { entity = Deserialize(message.Entity); }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to deserialize.");
                throw;
            }

            try { await Import(entity); }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to import.");
                throw;
            }
        }

        IEntity Deserialize(string serialized)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialized);
            var result = DomainType.CreateInstance<IEntity>();

            foreach (var field in data)
            {
                var property = DomainType.GetProperty(field.Key);
                if (property.PropertyType.IsA<IEntity>())
                    property = property.DeclaringType.GetProperty(property.Name + "Id");

                property.SetValue(result, field.Value.To(property.PropertyType));
            }

            return result;
        }

        async Task Import(IEntity entity)
        {
            var existing = await Database.GetOrDefault(entity.GetId(), DomainType);

            using (var scope = Database.CreateTransactionScope())
            {
                entity.GetType().GetProperty("IsNew").SetValue(entity, existing == null);
                entity.GetType().GetField("IsImmutable").SetValue(entity, false);

                await Database.Save(entity, SaveBehaviour.BypassAll);
                Log.Debug("Saved the " + entity.GetType().FullName + " " + entity.GetId());
                scope.Complete();
            }
        }
    }
}