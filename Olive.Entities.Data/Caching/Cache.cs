using System;
using System.Collections;

namespace Olive.Entities.Data
{
    public class Cache : ICache
    {
        readonly ICacheProvider CacheProvider;
        public Cache(ICacheProvider provider) => CacheProvider = provider;

        public DateTime? GetQueryTimestamp()
           => Database.Configuration.Cache.ConcurrencyAware ? DateTime.UtcNow : default(DateTime?);

        /// <summary>
        /// Adds a given entity to the cache.
        /// </summary>
        public void Add(IEntity entity)
        {
            if (entity.GetType().IsCacheable())
            {
                CacheProvider.Add(entity);
                ExpireLists(entity.GetType());
            }
        }

        /// <summary>
        /// Removes a given entity from the cache.
        /// </summary>
        public virtual void Remove(IEntity entity)
        {
            entity.InvalidateCachedReferences();

            foreach (var type in CacheDependentAttribute.GetDependentTypes(entity.GetType()))
                Remove(type, invalidateCachedReferences: true);

            if (entity.GetType().IsCacheable())
            {
                ExpireLists(entity.GetType());
                CacheProvider.Remove(entity);
            }
        }

        /// <summary>
        /// Removes all entities of a given types from the cache.
        /// </summary>
        public virtual void Remove(Type type, bool invalidateCachedReferences = false)
        {
            if (!type.IsCacheable()) return;

            ExpireLists(type);

            CacheProvider.Remove(type, invalidateCachedReferences);

            foreach (var inherited in type.Assembly.GetSubTypes(type, withDescendants: true))
                CacheProvider.Remove(inherited, invalidateCachedReferences);
        }

        public virtual void ExpireLists(Type type)
        {
            if (!type.IsCacheable()) return;

            for (var parentType = type; parentType != typeof(Entity); parentType = parentType.BaseType)
                CacheProvider.ExpireLists(type);
        }

        public virtual IEnumerable GetList(Type type, string key)
        {
            if (type.IsCacheable()) return null;
            return CacheProvider.GetList(type, key);
        }

        public void ClearAll() => CacheProvider.ClearAll();

        public void AddList(Type type, string key, IEnumerable list)
        {
            if (type.IsCacheable()) CacheProvider.AddList(type, key, list);
        }

        public bool IsUpdatedSince(IEntity instance, DateTime since) => CacheProvider.IsUpdatedSince(instance, since);

        public void UpdateRowVersion(IEntity entity) => CacheProvider.UpdateRowVersion(entity);

        public virtual TEntity Get<TEntity>(object id) where TEntity : IEntity
            => (TEntity)Get(typeof(TEntity), id.ToStringOrEmpty());

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public IEntity Get(Type type, string id)
        {
            if (!type.IsCacheable()) return null;
            var result = CacheProvider.Get(type, id);
            if (!(result is null)) return result;

            foreach (var t in type.Assembly.GetSubTypes(type))
            {
                result = Get(t, id);
                if (result != null) return result;
            }

            return null;
        }
    }
}