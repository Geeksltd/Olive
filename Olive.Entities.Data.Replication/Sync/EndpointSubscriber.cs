using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public class EndpointSubscriber
    {
        public Type DomainType { get; set; }
        public DestinationEndpoint Endpoint { get; }
        DateTime? RefreshRequestUtc;

        public EndpointSubscriber(DestinationEndpoint endpoint, Type domainType)
        {
            Endpoint = endpoint;
            DomainType = domainType;
        }

        protected IDatabase Database => Context.Current.Database();

        public async Task RefreshData()
        {
            var request = new RefreshMessage { TypeName = DomainType.FullName, RequestUtc = DateTime.UtcNow };
            RefreshRequestUtc = request.RequestUtc;
            await Endpoint.RefreshQueue.Publish(request);
        }

        internal Task Import(ReplicateDataMessage message)
        {
            if (message.CreationUtc < RefreshRequestUtc)
            {
                // Ignore this. We will receive a full table after this anyway.
                return Task.CompletedTask;
            }

            var entity = (IEntity)JsonConvert.DeserializeObject(message.Entity, DomainType);

            return Import(entity);
        }

        async Task Import(IEntity entity)
        {
            try
            {
                var existing = await Database.GetOrDefault(entity.GetId(), DomainType);
                if (existing != null)
                    await Database.Delete(existing);

                if (!entity.IsNew)
                    entity.GetType().GetProperty("IsNew").SetValue(entity, true);

                await Database.Save(entity, SaveBehaviour.BypassAll);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}