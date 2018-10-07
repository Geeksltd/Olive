﻿using System;
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
        object SyncLock = new object();

        Dictionary<Type, Dictionary<string, IEntity>> Types = new Dictionary<Type, Dictionary<string, IEntity>>();
        Dictionary<Type, Dictionary<string, IEnumerable>> Lists = new Dictionary<Type, Dictionary<string, IEnumerable>>();

        Dictionary<string, IEntity> GetEntities(Type type)
        {
            if (Types.TryGetValue(type, out var result)) return result;

            lock (SyncLock)
            {
                if (Types.TryGetValue(type, out result)) return result;

                result = new Dictionary<string, IEntity>();
                Types.Add(type, result);
                return result;
            }
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
            if (Types.TryGetValue(type, out var entities))
            {
                lock (entities) Types.Remove(type);

                if (invalidateCachedReferences)
                    entities.Do(e => e.Value.InvalidateCachedReferences());
            }
        }

        public void ExpireLists(Type type)
        {
            var lists = GetLists(type, autoCreate: false);
            if (lists != null) lock (lists) lists.Clear();
        }

        public IEnumerable GetList(Type type, string key)
        {
            var lists = GetLists(type);
            lock (lists)
            {
                if (lists.TryGetValue(key, out var result)) return result;
                else return null;
            }
        }

        public void AddList(Type type, string key, IEnumerable list)
        {
            var lists = GetLists(type);
            lock (lists) lists[key] = list;
        }

        public void ClearAll()
        {
            lock (SyncLock)
            {
                RowVersionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();
                Types.Clear();
                Lists.Clear();
            }
        }
    }
}