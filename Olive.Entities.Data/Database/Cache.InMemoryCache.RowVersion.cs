using System;
using System.Collections.Concurrent;

namespace Olive.Entities.Data
{
    partial class InMemoryCache
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

        public override bool IsUpdatedSince(IEntity instance, DateTime since)
        {
            var type = instance.GetType();
            if (!CanCache(type)) return false;

            var cache = RowVersionCache.GetOrDefault(type);
            return cache?.GetOrDefault(instance.GetId().ToString()) > since.Ticks;
        }

        public override void UpdateRowVersion(IEntity entity)
        {
            var type = entity.GetType();
            if (!CanCache(type)) return;

            var cache = RowVersionCache.GetOrAdd(type, t => new ConcurrentDictionary<string, long>());
            cache[entity.GetId().ToString()] = DateTime.UtcNow.Ticks;
        }
    }
}