using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides a cache of objects retrieved from the database.
    /// </summary>
    public partial class InMemoryCacheProvider : ICacheProvider, IQueryCacheProvider
    {
        // Concurrent at both levels: in the single-server cache mode one provider is shared by every request.
        ConcurrentDictionary<Type, ConcurrentDictionary<string, IEntity>> Types = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IEntity>>();
        ConcurrentDictionary<Type, IEnumerable> Lists = new ConcurrentDictionary<Type, IEnumerable>();
        ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> Queries = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();
        ConcurrentDictionary<Type, long> QueryResultsInvalidatedAt = new ConcurrentDictionary<Type, long>();
        int? maxCachedQueriesPerType;
        int MaxCachedQueriesPerType => maxCachedQueriesPerType ??= Config.Get("Database:Cache:MaxCachedQueriesPerType", 200);

        ConcurrentDictionary<string, IEntity> GetEntities(Type type) =>
            Types.GetOrAdd(type, t => new ConcurrentDictionary<string, IEntity>());

        public IEntity Get(Type entityType, string id)
        {
            // A read creates no map for a type that has never been cached.
            if (!Types.TryGetValue(entityType, out var entities)) return null;

            return entities.TryGetValue(id, out var result) ? result : null;
        }

        public void Add(IEntity entity)
        {
            var entities = GetEntities(entity.GetType());
            var id = entity.GetId().ToString();

            // Writes are locked, as TryUpdate() can't detect a concurrent replacement: it compares the instances
            // with Entity.Equals(), which matches any instance with the same ID. Reads remain lock-free.
            lock (entities)
            {
                entities.TryGetValue(id, out var existing);
                entities[id] = entity;

                // Invalidated once replaced, so that a reference reloading it finds the new instance.
                existing?.InvalidateCachedReferences();
            }
        }

        public void Remove(IEntity entity)
        {
            if (Types.TryGetValue(entity.GetType(), out var entities))
                entities.TryRemove(entity.GetId().ToString(), out _);
        }

        public void Remove(Type type, bool invalidateCachedReferences = false)
        {
            if (Types.TryRemove(type, out var entities))
            {
                if (invalidateCachedReferences)
                    entities.Do(e => e.Value.InvalidateCachedReferences());
            }
        }

        public void RemoveList(Type type) => Lists.TryRemove(type);

        public IEnumerable GetList(Type type) => Lists.GetOrDefault(type);

        public void AddList(Type type, IEnumerable list) => Lists[type] = list;

        public object GetQueryResult(Type type, string key) => Queries.GetOrDefault(type)?.GetOrDefault(key);

        public void SetQueryResult(Type type, string key, object result, DateTime? queryTime = null)
        {
            if (queryTime.HasValue && QueryResultsInvalidatedAt.GetOrDefault(type) > queryTime.Value.Ticks) return;

            var queries = Queries.GetOrAdd(type, t => new ConcurrentDictionary<string, object>());

            if (queries.Count >= MaxCachedQueriesPerType) queries.Clear();

            queries[key] = result;
        }

        public void RemoveQueryResults(Type type)
        {
            QueryResultsInvalidatedAt[type] = DateTime.UtcNow.Ticks;
            Queries.TryRemove(type, out _);
        }

        public void ClearAll()
        {
            RowVersionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();
            Types.Clear();
            Lists.Clear();
            Queries.Clear();
            QueryResultsInvalidatedAt.Clear();
        }
    }
}