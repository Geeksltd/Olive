using System;
using System.Threading.Tasks;

namespace Olive.Entities
{
    internal interface ICachedReference { void Invalidate(); }

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
    public class CachedReference<TId, TEntity> : ICachedReference
         where TEntity : Entity<TId>
         where TId : struct
    {
        [NonSerialized]
        TEntity Value;
        TId? Id;

        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity Get(TId? id)
        {
            if (!Id.Equals(id)) Value = null; // Different ID from the cache.
            Id = id;

            if (Value != null) return Value;
            return Task.Factory.RunSync(() => GetAsync(id));
        }

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity GetOrDefault(TId? id)
        {
            if (!Id.Equals(id)) Value = null; // Different ID from the cache.
            Id = id;

            if (Value != null) return Value;
            return Task.Factory.RunSync(() => GetOrDefaultAsync(id));
        }

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public async Task<TEntity> GetAsync(TId? id)
        {
            if (!Id.Equals(id)) Value = null; // Different ID from the cache.
            Id = id;

            if (Value != null) return Value;

            if (id == null) return null;

            var result = await Database.Get<TEntity>(id.ToString());

            if (!Database.AnyOpenTransaction())
            {
                Value = result;
                Value.RegisterCachedCopy(this);
                return result;
            }
            else return result;
        }

        /// <summary>
        /// Gets the entity record from a specified database call expression, if it exists, or null.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public async Task<TEntity> GetOrDefaultAsync(TId? id)
        {
            if (!Id.Equals(id)) Value = null; // Different ID from the cache.
            Id = id;

            if (Value != null) return Value;

            if (id == null) return null;

            var result = await Database.GetOrDefault<TEntity>(id.ToString());
            if (result == null) return result;

            if (!Database.AnyOpenTransaction())
            {
                Value = result;
                Value.RegisterCachedCopy(this);
                return result;
            }
            else return result;
        }

        protected void Bind(TEntity entity)
        {
            Id = entity?.ID ?? throw new ArgumentNullException(nameof(entity));
            Value = entity;

            if (!Database.AnyOpenTransaction())
                Value.RegisterCachedCopy(this);
        }

        void ICachedReference.Invalidate() => Value = null;
    }
}