using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides a cache of objects retrieved from the database.
    /// </summary>
    public partial class InMemoryCacheProvider : ICacheProvider
    {
        ConcurrentDictionary<Type, Dictionary<string, IEntity>> Types = new ConcurrentDictionary<Type, Dictionary<string, IEntity>>();
        ConcurrentDictionary<Type, IEnumerable> Lists = new ConcurrentDictionary<Type, IEnumerable>();

        Dictionary<string, IEntity> GetEntities(Type type) =>
            Types.GetOrAdd(type, t => new Dictionary<string, IEntity>());

        public IEntity Get(Type entityType, string id)
        {
            var entities = GetEntities(entityType);

            if (entities == null) return null;
            try
            {
                if (entities.TryGetValue(id, out var result))
                    return result;
            }
            catch
            {
                // A threading issue. No logging is needed.
                return Get(entityType, id);
            }

            return null;
        }

        public void Add(IEntity entity)
        {
            var entities = GetEntities(entity.GetType());

            lock (entities)
            {
                var id = entity.GetId().ToString();
                if (entities.ContainsKey(id))
                {
                    entities.GetOrDefault(id)?.InvalidateCachedReferences();
                    entities.Remove(id);
                }

                entities.Add(id, entity);
            }
        }

        public void Remove(IEntity entity)
        {
            var entities = GetEntities(entity.GetType());

            lock (entities)
                entities.Remove(entity.GetId().ToString());
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

        public void ClearAll()
        {
            RowVersionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();
            Types.Clear();
            Lists.Clear();
        }
    }
}