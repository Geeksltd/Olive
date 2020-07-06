using System;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class Database
    {
        internal async Task<IEntity> GetConcrete(object entityID, Type concreteType)
        {
            var result = Cache.Get(concreteType, entityID.ToString());
            if (result != null) return result;

            var timestamp = Cache.GetQueryTimestamp();

            result = await FromDatabase(entityID, concreteType);

            // Don't cache the result if it is fetched in a transaction.
            if (result != null) TryCache(result, timestamp);

            return result;
        }

        internal void TryCache(IEntity item, DateTime? queryTime)
        {
            if (AnyOpenTransaction()) return;
            if (queryTime.HasValue && Cache.IsUpdatedSince(item, queryTime.Value)) return;
            Cache.Add(item);
        }

        [EscapeGCop("I am the solution to this GCop warning")]
        async Task<IEntity> FromDatabase(object entityID, Type concreteType)
        {
            var result = await GetProvider(concreteType).Get(entityID);

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