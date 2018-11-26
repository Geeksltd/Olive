using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public class DataReplicationEndPointConsumer
    {
        protected static IDatabase Database => Context.Current.Database();

        protected static async Task RefreshData(Type type)
        {
            await EventBus.Purge(type.FullName);

            var records = await GetAll(type);
            await Database.BulkInsert(records);
        }

        static async Task<Entity[]> GetAll(Type type)
        {
            // TODO: Fetch from the Api
            throw new NotImplementedException();
        }

        protected static async Task Subscribe(Type type)
        {
            if (await Database.Of(type).None())
                await RefreshData(type);

            EventBus.Subscribe<ReplicateDataMessage>(type.FullName, Import);
        }

        static Task Import(ReplicateDataMessage message)
        {
            // TODO: Process the message and apply on the local DB
            throw new NotImplementedException();
        }
    }
}