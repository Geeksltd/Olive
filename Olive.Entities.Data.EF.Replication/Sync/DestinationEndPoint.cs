using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.EF.Replication
{
    public abstract partial class DestinationEndpoint
	{
        public readonly IEventBusQueue PublishQueue, RefreshQueue;
        protected DbContext DbContext;
        public string Table;
		public string Schema;
        ConcurrentDictionary<string, DateTime> ResetRequestUtcs = new ConcurrentDictionary<string, DateTime>();
        Dictionary<string, EndpointSubscriber> Subscribers = new Dictionary<string, EndpointSubscriber>();
		Assembly DomainAssembly;


		public virtual string QueueUrl => Data.Replication.QueueUrlProvider.UrlProvider.GetUrl(GetType());

        protected DestinationEndpoint(Assembly domainAssembly, DbContext _dbContext)
		{
			DomainAssembly = domainAssembly;
			DbContext = _dbContext;
			PublishQueue = EventBus.Queue(QueueUrl);
            RefreshQueue = EventBus.Queue(QueueUrl.TrimEnd(".fifo") + "-REFRESH.fifo");
        }

		protected EndpointSubscriber Register(string domainType)
		{
			var type = DomainAssembly.GetType(domainType)
				?? throw new Exception(DomainAssembly.FullName + " does not define the type " + domainType);


            Schema = type.GetCustomAttribute<TableAttribute>(inherit: false)?.Schema;
            Table = GetTableName(type);

			var result = new EndpointSubscriber(DbContext, this, type);
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

        public async Task Subscribe(bool isRefreshMessageRequired = false)
        {
            if(isRefreshMessageRequired)
                await EnsureRefreshData();

            PublishQueue.Subscribe<ReplicateDataMessage>(Import);
        }

        public async Task PullAll()
        {
            var start = LocalTime.Now;
            await PublishQueue.PullAll<ReplicateDataMessage>(Import);
            Log.For(this).Info("Pulled from queue in " + LocalTime.Now.Subtract(start).ToNaturalTime());
        }

        public virtual Task Handle(string message) => Import(Newtonsoft.Json.JsonConvert.DeserializeObject<ReplicateDataMessage>(message));

        async Task EnsureRefreshData()
        {
            foreach (var item in Subscribers.Values)
			{
                var sqlRemoveData = $"SELECT COUNT(*) FROM {Schema.WithSuffix(".")}{Table}";

				var count = await DbContext.Database.ExecuteSqlRawAsync(sqlRemoveData);

                if (count <= 0)
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

		public string GetTableName(Type entityType)
		{
			var result = entityType.GetCustomAttribute<TableAttribute>(inherit: false)?.Name;
			if (result.HasValue()) return result;

			return entityType.Name.ToPlural().ToPascalCaseId();
		}
	}
}
