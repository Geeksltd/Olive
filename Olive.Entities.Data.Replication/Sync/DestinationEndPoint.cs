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
        public readonly IEventBusQueue PublishQueue, RefreshQueue;
        protected IDatabase Database => Context.Current.Database();
        ConcurrentDictionary<string, DateTime> ResetRequestUtcs = new ConcurrentDictionary<string, DateTime>();
        Dictionary<string, EndpointSubscriber> Subscribers = new Dictionary<string, EndpointSubscriber>();

        public virtual string QueueUrl => Data.Replication.QueueUrlProvider.UrlProvider.GetUrl(GetType());

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

            PublishQueue.Subscribe<ReplicateDataMessage>(ImportUnderOwnReference);
        }

        public async Task Subscribe(bool isRefreshMessageRequired = false)
        {
            if(isRefreshMessageRequired)
                await EnsureRefreshData();

            PublishQueue.Subscribe<ReplicateDataMessage>(ImportUnderOwnReference);
        }

        public async Task PullAll()
        {
            var start = LocalTime.Now;
            await PublishQueue.PullAll<ReplicateDataMessage>(ImportUnderOwnReference);
            Log.For(this).Info("Pulled from queue in " + LocalTime.Now.Subtract(start).ToNaturalTime());
        }

        public virtual Task Handle(string message)
            => ImportUnderOwnReference(Newtonsoft.Json.JsonConvert.DeserializeObject<ReplicateDataMessage>(message));

        /// <summary>
        /// Imports under a reference code of its own rather than adopting the one the message carries.
        /// An import is caused by the saving request but is not part of it, and adopting its code would
        /// spread it: importing republishes to the next service, which would adopt it in turn. Recording
        /// the publisher's code as the *cause* instead is an edge, so it travels one hop and no further.
        /// </summary>
        async Task ImportUnderOwnReference(ReplicateDataMessage message)
        {
            using (Log.UseReference(null, causedBy: message?.ReferenceCode))
                await Import(message);
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
                // Reported here, under the import's own code. Rethrown as LoggedException so the queue
                // subscriber stops the FIFO loop without filing it again under the publisher's code.
                Log.For(this).Error(ex, $"Failed to import ReplicateDataMessage {message.Entity}|TypeFullName : {message.TypeFullName}");
                throw new LoggedException(ex);
            }
        }
    }
}
