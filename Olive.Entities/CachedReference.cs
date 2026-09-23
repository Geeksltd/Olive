using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Entities
{
    /// <summary>
    /// Invalidates every cached reference in this process at once.
    /// </summary>
    public static class CachedReferences
    {
        static int InvalidationGeneration;

        internal static int Generation => Volatile.Read(ref InvalidationGeneration);

        /// <summary>
        /// Invalidates all cached references, so the next call to each of them loads its record again.
        /// It is called when the whole database cache is cleared, as the instances they hold are no longer the
        /// cached ones. Unlike Entity.InvalidateCachedReferences(), it needs no access to those instances, so it
        /// also works for a cache provider that does not keep them in memory (e.g. Redis).
        /// </summary>
        public static void InvalidateAll() => Interlocked.Increment(ref InvalidationGeneration);
    }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    [Serializable]
    public class CachedReference<TEntity> : CachedReference<Guid, TEntity>
        where TEntity : GuidEntity
    { }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    [Serializable]
    public class CachedReference<TId, TEntity> : CachedReferenceBase<TId?, TEntity>
         where TEntity : Entity<TId>
         where TId : struct
    {
        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity Get(TId? id) => GetEntity(id);

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity GetOrDefault(TId? id) => GetEntityOrDefault(id);

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public Task<TEntity> GetAsync(TId? id) => GetEntityAsync(id);

        /// <summary>
        /// Gets the entity record from a specified database call expression, if it exists, or null.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public Task<TEntity> GetOrDefaultAsync(TId? id) => GetEntityOrDefaultAsync(id);

        /// <summary>
        /// Gets the entity record from a specified loader, when it is not already cached.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity Get(TId? id, Func<Task<TEntity>> loader) => GetEntity(id, loader);

        /// <summary>
        /// Gets the entity record from a specified loader, when it is not already cached.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public Task<TEntity> GetAsync(TId? id, Func<Task<TEntity>> loader) => GetEntityAsync(id, loader);

        protected override bool IsSameId(TId? first, TId? second) => first.Equals(second);

        protected override bool IsEmpty(TId? id) => id == null;

        protected override TId? GetId(TEntity entity) => entity.ID;
    }

    /// <summary>
    /// Provides immediate access to retrieved entities with a string primary key. It is aware of deletes and updates.
    /// </summary>
    [Serializable]
    public class StringCachedReference<TEntity> : CachedReferenceBase<string, TEntity>
         where TEntity : Entity<string>
    {
        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity Get(string id) => GetEntity(id);

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity GetOrDefault(string id) => GetEntityOrDefault(id);

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public Task<TEntity> GetAsync(string id) => GetEntityAsync(id);

        /// <summary>
        /// Gets the entity record from a specified database call expression, if it exists, or null.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public Task<TEntity> GetOrDefaultAsync(string id) => GetEntityOrDefaultAsync(id);

        /// <summary>
        /// Gets the entity record from a specified loader, when it is not already cached.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity Get(string id, Func<Task<TEntity>> loader) => GetEntity(id, loader);

        /// <summary>
        /// Gets the entity record from a specified loader, when it is not already cached.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public Task<TEntity> GetAsync(string id, Func<Task<TEntity>> loader) => GetEntityAsync(id, loader);

        // Exact match only: under a case-sensitive collation "gb" and "GB" are different records.
        protected override bool IsSameId(string first, string second) => string.Equals(first, second);

        protected override bool IsEmpty(string id) => id.IsEmpty();

        protected override string GetId(TEntity entity) => entity.ID;
    }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    [Serializable]
    public abstract class CachedReferenceBase<TKey, TEntity>
         where TEntity : Entity
    {
        /// <summary>
        /// A loaded record, with the versions it was loaded at. It is replaced as a whole, never changed, so a reader
        /// never sees the record of one load with the versions of another.
        /// </summary>
        sealed class Snapshot
        {
            public readonly TEntity Value;
            readonly int Version, Generation;
            readonly bool TracksRecord;

            /// <param name="tracksRecord">Whether invalidating the record invalidates this snapshot.</param>
            public Snapshot(TEntity value, bool tracksRecord = true)
            {
                // Read before the snapshot is published, so an invalidation in between is not missed.
                Version = value.CachedReferencesVersion;
                Generation = CachedReferences.Generation;
                TracksRecord = tracksRecord;
                Value = value;
            }

            /// <summary>
            /// Neither the record (when tracked) nor all cached references have been invalidated since it was loaded.
            /// </summary>
            public bool IsCurrent =>
                (!TracksRecord || Version == Value.CachedReferencesVersion) && Generation == CachedReferences.Generation;
        }

        [NonSerialized]
        volatile Snapshot Loaded;

        // Not serialized: it is only the key of Loaded, which isn't either. Serialized here in the base class, it would
        // be named differently from the Id field of earlier versions, so the entities they stored (e.g. in Redis, via
        // BinaryFormatter) could no longer be deserialized.
        [NonSerialized]
        TKey Id;

        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// The cached record, or null if nothing is cached or it has since been invalidated.
        /// </summary>
        TEntity Cached => Loaded is Snapshot loaded && loaded.IsCurrent ? loaded.Value : null;

        /// <summary>
        /// Caches a loaded record, unless it is loaded in a transaction.
        /// </summary>
        void SetCached(TEntity entity)
        {
            // A record that is not found is not cached, as there would be nothing to invalidate a cached null.
            if (entity == null) return;

            if (Database.AnyOpenTransaction()) return;

            Loaded = new Snapshot(entity);
        }

        protected abstract bool IsSameId(TKey first, TKey second);

        protected abstract bool IsEmpty(TKey id);

        protected abstract TKey GetId(TEntity entity);

        /// <summary>
        /// Returns the cached record for a specified ID, or null if it is not cached.
        /// </summary>
        TEntity GetCached(TKey id)
        {
            if (!IsSameId(Id, id)) Loaded = null; // Different ID from the cache.
            Id = id;

            return Cached;
        }

        protected TEntity GetEntity(TKey id) =>
            GetCached(id) ?? LoadSync(id, () => GetEntityAsync(id));

        protected TEntity GetEntityOrDefault(TKey id) =>
            GetCached(id) ?? LoadSync(id, () => GetEntityOrDefaultAsync(id));

        protected TEntity GetEntity(TKey id, Func<Task<TEntity>> loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            return GetCached(id) ?? LoadSync(id, () => GetEntityAsync(id, loader));
        }

        // An empty ID has no record, so there is nothing to load, and no thread to block for it.
        TEntity LoadSync(TKey id, Func<Task<TEntity>> load) => IsEmpty(id) ? null : Task.Factory.RunSync(load);

        protected async Task<TEntity> GetEntityAsync(TKey id)
        {
            var result = GetCached(id);
            if (result != null) return result;

            if (IsEmpty(id)) return null;

            result = await Database.Get<TEntity>(id.ToString()).ConfigureAwait(false);
            SetCached(result);

            return result;
        }

        protected async Task<TEntity> GetEntityOrDefaultAsync(TKey id)
        {
            var result = GetCached(id);
            if (result != null) return result;

            if (IsEmpty(id)) return null;

            result = await Database.GetOrDefault<TEntity>(id.ToString()).ConfigureAwait(false);
            SetCached(result);

            return result;
        }

        /// <summary>
        /// Gets the record from a specified loader, when it is not already cached.
        /// The loader decides how the record is found and how a missing record is reported, so, unlike
        /// GetOrDefault(), database errors are not necessarily swallowed.
        /// </summary>
        protected async Task<TEntity> GetEntityAsync(TKey id, Func<Task<TEntity>> loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));

            var result = GetCached(id);
            if (result != null) return result;

            if (IsEmpty(id)) return null;

            result = await loader().ConfigureAwait(false);
            SetCached(result);

            return result;
        }

        // Invoked via reflection by AssociationInclusion to bind eagerly loaded (Include) associations.
        protected void Bind(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            BindTo(GetId(entity), entity);
        }

        /// <summary>
        /// Binds a loaded record to the ID it is read by, which may differ from its own ID, e.g. "gb" for the record
        /// "GB" under a case-insensitive collation, so that reading it by that ID is served without loading it again.
        /// Invoked via reflection by DatabaseIncludeExtensions (Including).
        /// </summary>
        protected void BindTo(TKey id, TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Id = id;

            // By design, a record bound inside a transaction is not tied to the invalidation of its record.
            Loaded = new Snapshot(entity, tracksRecord: !Database.AnyOpenTransaction());
        }
    }
}
