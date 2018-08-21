using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities.Data
{
    public abstract class Cache
    {
        public static Cache Instance = new InMemoryCache();

        /// <summary>
        /// Gets the current cache.
        /// </summary>
        public static Cache Current => Instance;

        public static bool CanCache(Type type)
          => CacheObjectsAttribute.IsEnabled(type) ?? Database.Configuration.Cache.Enabled;

        internal static DateTime? GetQueryTimestamp()
           => Database.Configuration.Cache.ConcurrencyAware ? DateTime.UtcNow : default(DateTime?);

        /// <summary>
        /// Adds a given entity to the cache.
        /// </summary>
        public void Add(IEntity entity)
        {
            if (CanCache(entity.GetType()))
            {
                DoAdd(entity);
                ExpireLists(entity.GetType());
            }
        }

        protected abstract void DoAdd(IEntity entity);

        /// <summary>
        /// Removes a given entity from the cache.
        /// </summary>
        public virtual void Remove(IEntity entity)
        {
            entity.InvalidateCachedReferences();

            foreach (var type in CacheDependentAttribute.GetDependentTypes(entity.GetType()))
                Remove(type, invalidateCachedReferences: true);

            if (CanCache(entity.GetType()))
            {
                ExpireLists(entity.GetType());
                DoRemove(entity);
            }

            if (this != Current) Current.Remove(entity);
        }

        protected abstract void DoRemove(IEntity entity);

        /// <summary>
        /// Removes all entities of a given types from the cache.
        /// </summary>
        public virtual void Remove(Type type, bool invalidateCachedReferences = false)
        {
            if (!CanCache(type)) return;

            ExpireLists(type);

            DoRemove(type, invalidateCachedReferences);

            foreach (var inherited in type.Assembly.GetSubTypes(type, withDescendants: true))
                DoRemove(inherited, invalidateCachedReferences);

            if (this != Current)
                Current.Remove(type, invalidateCachedReferences);
        }

        protected abstract void DoRemove(Type type, bool invalidateCachedReferences = false);

        public virtual void ExpireLists(Type type)
        {
            if (!CanCache(type)) return;

            for (var parentType = type; parentType != typeof(Entity); parentType = parentType.BaseType)
            {
                DoExpireLists(type);
            }

            if (this != Current) Current.ExpireLists(type);
        }

        protected abstract void DoExpireLists(Type type);

        public virtual IEnumerable GetList(Type type, string key)
        {
            if (!CanCache(type)) return null;
            return DoGetList(type, key);
        }

        protected abstract IEnumerable DoGetList(Type type, string key);

        public abstract void ClearAll();

        public void AddList(Type type, string key, IEnumerable list)
        {
            if (CanCache(type)) DoAddList(type, key, list);
        }

        protected abstract void DoAddList(Type type, string key, IEnumerable list);

        public abstract bool IsUpdatedSince(IEntity instance, DateTime since);

        public abstract void UpdateRowVersion(IEntity entity);

        public virtual TEntity Get<TEntity>(object id) where TEntity : IEntity
            => (TEntity)Get(typeof(TEntity), id.ToStringOrEmpty());

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public IEntity Get(Type type, string id)
        {
            if (!CanCache(type)) return null;
            var result = DoGet(type, id);
            if (!(result is null)) return result;

            foreach (var t in type.Assembly.GetSubTypes(type))
            {
                result = Get(t, id);
                if (result != null) return result;
            }

            return null;
        }

        protected abstract IEntity DoGet(Type entityType, string id);
    }
}