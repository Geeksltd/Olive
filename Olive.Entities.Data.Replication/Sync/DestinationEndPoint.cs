using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract partial class DestinationEndpoint
    {
        Assembly DomainAssembly;
        internal IEventBusQueue PublishQueue, RefreshQueue;
        protected IDatabase Database => Context.Current.Database();
        ConcurrentDictionary<string, DateTime> ResetRequestUtcs = new ConcurrentDictionary<string, DateTime>();
        Dictionary<string, EndpointSubscriber> Subscribers = new Dictionary<string, EndpointSubscriber>();

        public abstract string QueueUrl { get; }

        protected DestinationEndpoint(Assembly domainAssembly)
        {
            DomainAssembly = domainAssembly;
            PublishQueue = EventBus.Queue(QueueUrl);
            RefreshQueue = EventBus.Queue(QueueUrl.TrimEnd(".fifo") + "-REFRESH.fifo");
        }

        protected EndpointSubscriber Register(string domainType)
        {
            var type = DomainAssembly.GetType(domainType)
                ?? throw new Exception(DomainAssembly.FullName + " does not define the type " + domainType);

            var result = new EndpointSubscriber(this, type);
            Subscribers.Add(domainType, result);
            return result;
        }

        /// <summary> It will start listening to queue messages to keep the local database up to date
        /// with the changes in the People. But before it starts that, if the local table
        /// is empty, it will fetch the full data. </summary>
        public async Task Subscribe()
        {
            await EnsureRefreshData();

            PublishQueue.Subscribe<ReplicateDataMessage>(Import);
        }

        public async Task PullAll()
        {
            var start = LocalTime.Now;
            await PublishQueue.PullAll<ReplicateDataMessage>(Import);
            Log.For(this).Info("Pulled from queue in " + LocalTime.Now.Subtract(start).ToNaturalTime());
        }

        async Task EnsureRefreshData()
        {
            foreach (var item in Subscribers.Values)
            {
                if (await Database.Of(item.DomainType).None())
                    await item.RefreshData();
            }
        }

        async Task Import(ReplicateDataMessage message)
        {
            if (message == null) return;

            try
            {
                await Subscribers[message.TypeFullName].Import(message);
            }
            catch (Exception ex)
            {
                Log.For(this).Error(ex, "Failed to import ReplicateDataMessage " + message.Entity);
                throw;
            }
        }
    }
}