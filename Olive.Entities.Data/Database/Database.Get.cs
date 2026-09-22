using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class Database
    {
        static readonly AsyncLocal<Type[]> TypesLoadingAllRecords = new AsyncLocal<Type[]>();

        readonly ConcurrentDictionary<Type, Lazy<Task<IEntity[]>>> AllRecordsLoads =
            new ConcurrentDictionary<Type, Lazy<Task<IEntity[]>>>();

        internal Task<IEntity> GetConcrete(object entityID, Type concreteType) =>
            GetConcrete(entityID, concreteType, nullIfMissing: false);

        async Task<IEntity> GetConcrete(object entityID, Type concreteType, bool nullIfMissing)
        {
            var result = Cache.Get(concreteType, entityID.ToString());
            if (result != null) return result;

            var loadsAllRecords = CanLoadAllRecords(concreteType);

            if (loadsAllRecords)
            {
                var id = entityID.ToString();

                result = (await LoadAllRecords(concreteType)).FirstOrDefault(x => x.GetId().ToString() == id);
                if (result != null) return result;

                // Not in the loaded list: the record may be soft deleted, added since the list was loaded,
                // or the ID may be in a different text form. So it is still loaded by its ID, as before.
            }

            var timestamp = Cache.GetQueryTimestamp();

            result = await FromDatabase(entityID, concreteType, nullIfMissing);
            if (result == null) return null;

            // Looked up by a different text form of its ID (e.g. "gb" for "GB"), the record may already be cached.
            // Replacing it would invalidate the references to it, and discard the loaded list of its type.
            var cached = Cache.Get(result.GetType(), result.GetId().ToString());
            if (cached != null) return cached;

            // A soft deleted record is legitimately missing from the loaded list, so that list remains valid.
            var keepList = loadsAllRecords && result is Entity entity && SoftDeleteAttribute.IsMarked(entity);

            // Don't cache the result if it is fetched in a transaction.
            TryCache(result, timestamp, keepList);

            return result;
        }

        /// <summary>
        /// Determines whether all records of a [CacheAllRecords] type can be loaded into the cache in one query now.
        /// Not inside a transaction or when the type isn't cacheable, as the loaded list would not be kept, nor while
        /// the records of that type are already being loaded (e.g. a record's ToString() reading another record).
        /// Nor within a DatabaseContext, whose connection may not be the one the loaded list was queried from.
        /// </summary>
        internal bool CanLoadAllRecords(Type type)
        {
            if (type.IsAbstract || !CacheAllRecordsAttribute.IsEnabled(type)) return false;
            if (!Cache.IsCacheable(type)) return false;
            if (SoftDeleteAttribute.Context.ShouldByPassSoftDelete()) return false;
            if (TypesLoadingAllRecords.Value?.Contains(type) == true) return false;
            if (DatabaseContext.Current != null) return false;
            if (AnyOpenTransaction()) return false;

            // The records are loaded through Context.Current.Database(), so they are only kept in the cache of this
            // instance if that is this instance. It is not, for example, outside a request in the multi-server cache
            // mode, where every call gets a new Database with an empty cache: loading all records would then be
            // repeated for every lookup, so each record is loaded by its ID instead.
            return ReferenceEquals(Context.Current.Database(), this);
        }

        /// <summary>
        /// Loads all records of a [CacheAllRecords] type, from the cache if already loaded.
        /// Concurrent callers share a single load of the same type.
        /// </summary>
        internal async Task<IEntity[]> LoadAllRecords(Type type)
        {
            // While loading another type, never wait for a load started elsewhere: that load could be waiting for
            // this one (e.g. two types whose ToString() read each other), so load it independently instead.
            if (TypesLoadingAllRecords.Value?.Any() == true) return await LoadAllRecordsNow(type);

            var load = AllRecordsLoads.GetOrAdd(type, t => new Lazy<Task<IEntity[]>>(() => LoadAllRecordsNow(t)));

            try { return await load.Value; }
            finally
            {
                ((ICollection<KeyValuePair<Type, Lazy<Task<IEntity[]>>>>)AllRecordsLoads)
                    .Remove(new KeyValuePair<Type, Lazy<Task<IEntity[]>>>(type, load));
            }
        }

        async Task<IEntity[]> LoadAllRecordsNow(Type type)
        {
            var loading = TypesLoadingAllRecords.Value ?? new Type[0];
            TypesLoadingAllRecords.Value = loading.Concat(type).ToArray();

            try { return await Of(type).GetList(); }
            finally { TypesLoadingAllRecords.Value = loading; }
        }

        /// <summary>
        /// Gets the record with the specified ID, or null if there is no such record.
        /// Unlike GetOrDefault(), errors other than the record not being found are not swallowed, when the data
        /// provider reports a missing record with a DataException (as the ADO.NET providers do) or with null.
        /// For an interface type, where the implementing types are tried in turn, it behaves like GetOrDefault().
        /// </summary>
        internal async Task<IEntity> FindById(object entityID, Type type)
        {
            if (entityID.ToStringOrEmpty().IsEmpty()) return null;

            if (NeedsTypeResolution(type)) return await GetOrDefault(entityID, type);

            return await GetConcrete(entityID, type, nullIfMissing: true);
        }

        /// <summary>
        /// Adds an item to the cache, unless it is loaded in a transaction or has changed since it was queried.
        /// Returns whether it was added.
        /// </summary>
        internal bool TryCache(IEntity item, DateTime? queryTime, bool keepList = false)
        {
            if (AnyOpenTransaction()) return false;
            if (queryTime.HasValue && Cache.IsUpdatedSince(item, queryTime.Value)) return false;

            // Keeping the loaded list needs the built-in Cache. Other ICache implementations discard it, which is safe.
            if (keepList && Cache is Cache cache) cache.AddKeepingList(item);
            else Cache.Add(item);

            return true;
        }

        [EscapeGCop("I am the solution to this GCop warning")]
        async Task<IEntity> FromDatabase(object entityID, Type concreteType, bool nullIfMissing = false)
        {
            IEntity result;

            try { result = await GetProvider(concreteType).Get(entityID); }
            catch (DataException) when (nullIfMissing)
            {
                // This is how the data provider reports that no record has this ID.
                return null;
            }

            if (result != null) await Entity.Services.RaiseOnLoaded(result as Entity);

            return result;
        }

        /// <summary>
        /// Gets an Entity of the given type with the given Id from the database.
        /// If the specified ID is null or empty string, then a null entity will be returned.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>        
        /// <param name="entityId">The primary key value of the object to load in string format.</param>
        public async Task<T> Get<T>(string entityId) where T : IEntity
        {
            if (entityId.IsEmpty()) return default(T);
            else return (T)await Get(entityId, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> Get<T>(Guid id) where T : IEntity
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"Could not load the {typeof(T).Name} because the given objectID is empty.");

            return (T)await Get(id, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the specified ID is null, then a null entity will be returned.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> Get<T>(Guid? id) where T : IEntity
        {
            if (id.HasValue) return await Get<T>(id.Value);
            else return default(T);
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the specified ID is null, then a null entity will be returned.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> Get<T>(int? id) where T : IEntity<int>
        {
            if (id == null) return default(T);
            return (T)await Get(id.Value, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the specified ID is null, then a null entity will be returned.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> Get<T>(short? id) where T : IEntity<short>
        {
            if (id == null) return default(T);
            return (T)await Get(id.Value, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the specified ID is null, then a null entity will be returned.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> Get<T>(long? id) where T : IEntity<long>
        {
            if (id == null) return default(T);
            return (T)await Get(id.Value, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the specified ID is null, then a null entity will be returned.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> Get<T>(byte? id) where T : IEntity<byte>
        {
            if (id == null) return default(T);
            return (T)await Get(id.Value, typeof(T));
        }

        public async Task<T> Get<T>(int id) where T : IEntity<int> => (T)await Get(id, typeof(T));

        public async Task<T> Get<T>(long id) where T : IEntity<long> => (T)await Get(id, typeof(T));

        public async Task<T> Get<T>(byte id) where T : IEntity<byte> => (T)await Get(id, typeof(T));

        public async Task<T> Get<T>(short id) where T : IEntity<short> => (T)await Get(id, typeof(T));

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <param name="entityID">The primary key value of the object to load.</param>
        public async Task<IEntity<Guid>> Get(Guid entityID, Type objectType)
            => await Get((object)entityID, objectType) as IEntity<Guid>;

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <param name="entityID">The primary key value of the object to load.</param>
        public async Task<IEntity> Get(object entityID, Type objectType)
        {
            if (objectType == null) return null;

            IEntity result = null;

            if (NeedsTypeResolution(objectType))
            {
                foreach (var provider in ProviderConfig.ResolveDataProviders(objectType))
                {
                    try
                    {
                        if (!provider.EntityType.IsInterface && !provider.EntityType.IsAbstract)
                        {
                            result = Cache.Get(provider.EntityType, entityID.ToString());
                            if (result != null) return result;
                        }

                        result = await provider.Get(entityID);
                        if (result != null) break;
                    }
                    catch
                    {
                        // No logging is needed
                        continue;
                    }
                }
            }
            else
            {
                result = await GetConcrete(entityID, objectType);
            }

            if (result != null) return result;
            else
                throw new ArgumentException($"Could not load the {objectType.FullName} instance with the ID of {entityID}.");
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the key does not exist, it will return null, rather than throwing an exception.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public async Task<T> GetOrDefault<T>(object id) where T : IEntity => (T)await GetOrDefault(id, typeof(T));

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the key does not exist, it will return null, rather than throwing an exception.
        /// </summary>
        /// <param name="type">The type of the object to get</param>
        /// <param name="id">The primary key value of the object to load.</param>        
        public async Task<IEntity> GetOrDefault(object id, Type type)
        {
            if (id.ToStringOrEmpty().IsEmpty()) return null;

            try { return await Get(id, type); }
            catch
            {
                // No logging is needed.
                return null;
            }
        }
    }
}