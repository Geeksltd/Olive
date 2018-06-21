using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides a cache of objects retrieved from the database.
    /// </summary>
    public partial class Cache
    {
        object SyncLock = new object();
        public static Cache Instance = new Cache();
        Dictionary<Type, Dictionary<string, IEntity>> Types = new Dictionary<Type, Dictionary<string, IEntity>>();
        Dictionary<Type, Dictionary<string, IEnumerable>> Lists = new Dictionary<Type, Dictionary<string, IEnumerable>>();

        internal static DateTime? GetQueryTimestamp()
            => Database.Configuration.Cache.ConcurrencyAware ? DateTime.UtcNow : default(DateTime?);

        public static bool CanCache(Type type)
            => CacheObjectsAttribute.IsEnabled(type) ?? Database.Configuration.Cache.Enabled;

        /// <summary>
        /// Gets the current cache.
        /// </summary>
        public static Cache Current => Instance;

        Dictionary<string, IEntity> GetEntities(Type type)
        {
            var result = Types.TryGet(type);

            if (result == null)
            {
                lock (SyncLock)
                {
                    result = Types.TryGet(type);

                    if (result == null)
                    {
                        result = new Dictionary<string, IEntity>();
                        Types.Add(type, result);
                    }
                }
            }

            return result;
        }

        Dictionary<string, IEnumerable> GetLists(Type type, bool autoCreate = true)
        {
            var result = Lists.TryGet(type);

            if (result == null && autoCreate)
            {
                lock (SyncLock)
                {
                    result = Lists.TryGet(type);
                    if (result == null)
                    {
                        result = new Dictionary<string, IEnumerable>();
                        Lists.Add(type, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public virtual IEntity Get(string id)
        {
            try
            {
                foreach (var type in Types.Keys.ToArray().Where(t => t.IsA<GuidEntity>()))
                {
                    var result = Get(type, id);
                    if (result != null) return result;
                }
            }
            catch
            {
                // No logging is needed.
            }

            return null;
        }

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public virtual TEntity Get<TEntity>(object id) where TEntity : IEntity => (TEntity)Get(typeof(TEntity), id.ToStringOrEmpty());

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public virtual IEntity Get(Type entityType, string id)
        {
            if (!CanCache(entityType)) return null;

            var entities = GetEntities(entityType);

            if (entities.ContainsKey(id))
            {
                try
                {
                    return entities[id];
                }
                catch (KeyNotFoundException)
                {
                    // A threading issue.
                    return Get(entityType, id);
                }
            }
            else
            {
                foreach (var type in entityType.Assembly.GetSubTypes(entityType))
                {
                    var result = Get(type, id);
                    if (result != null) return result;
                }

                return null;
            }
        }

        /// <summary>
        /// Adds a given entity to the cache.
        /// </summary>
        public virtual void Add(IEntity entity)
        {
            if (!CanCache(entity.GetType())) return;

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

            if (!CanCache(entity.GetType())) return;

            var entities = GetEntities(entity.GetType());

            lock (entities)
            {
                var id = entity.GetId().ToString();

                if (entities.ContainsKey(id)) entities.Remove(id);

                ExpireLists(entity.GetType());
            }

            if (this != Current) Current.Remove(entity);
        }

        /// <summary>
        /// Removes all entities of a given types from the cache.
        /// </summary>
        public virtual void Remove(Type type, bool invalidateCachedReferences = false)
        {
            if (!CanCache(type)) return;

            lock (SyncLock)
            {
                foreach (var inherited in Types.Keys.Where(t => t.BaseType == type).ToList())
                    Remove(inherited, invalidateCachedReferences);
            }

            if (Types.ContainsKey(type))
            {
                lock (SyncLock)
                {
                    if (Types.ContainsKey(type))
                    {
                        var entities = Types[type];
                        lock (entities)
                        {
                            Types.Remove(type);
                            ExpireLists(type);

                            if (invalidateCachedReferences)
                                entities.Do(e => e.Value.InvalidateCachedReferences());
                        }
                    }
                }
            }

            if (this != Current)
                Current.Remove(type, invalidateCachedReferences);
        }

        public virtual void ExpireLists(Type type)
        {
            if (!CanCache(type)) return;

            for (var parentType = type; parentType != typeof(Entity); parentType = parentType.BaseType)
            {
                var lists = GetLists(parentType, autoCreate: false);

                if (lists != null) lock (lists) lists.Clear();
            }

            if (this != Current) Current.ExpireLists(type);
        }

        public virtual IEnumerable GetList(Type type, string key)
        {
            if (!CanCache(type)) return null;

            var lists = GetLists(type);
            lock (lists)
            {
                if (lists.ContainsKey(key)) return lists[key];
                else return null;
            }
        }

        public virtual void AddList(Type type, string key, IEnumerable list)
        {
            if (!CanCache(type)) return;

            var lists = GetLists(type);

            lock (lists) lists[key] = list;
        }

        public virtual void ClearAll()
        {
            lock (SyncLock)
            {
                RowVersionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();
                Types.Clear();
                Lists.Clear();
            }
        }

        internal int CountAllObjects() => Types.Sum(t => t.Value.Count);
    }
}