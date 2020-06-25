using System;
using System.Collections.Concurrent;

namespace Olive.Entities.Data
{
    partial class InMemoryCacheProvider
    {
        // Note: This feature can prevent a rare concurrency issue in highly concurrent applications.
        // But it comes at the cost of performance degradation.
        // If your application doesn't have extremely concurrent processing,
        // with multiple threads reading and updating the same records at the same time,
        // you can disable it in web.config to improve performance.

        // In highly concurrent systems the following scenario can happen.
        //      A GET call loads a record from DB.
        //      It then adds it to the cache.
        //      If that record is updated in between the two steps above, then bad data is added to the cache.

        internal ConcurrentDictionary<Type, ConcurrentDictionary<string, long>> RowVersionCache
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();

        public bool IsUpdatedSince(IEntity instance, DateTime since)
        {
            var cache = RowVersionCache.GetOrDefault(instance.GetType());
            return cache?.GetOrDefault(instance.GetId().ToString()) > since.Ticks;
        }

        public void UpdateRowVersion(IEntity entity)
        {
            var cache = RowVersionCache.GetOrAdd(entity.GetType(), t => new ConcurrentDictionary<string, long>());
            cache[entity.GetId().ToString()] = DateTime.UtcNow.Ticks;
        }
    }
}