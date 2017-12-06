namespace Olive.Entities
{
    internal interface ICachedReference { void Invalidate(); }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    public class CachedReference<TEntity> : CachedReference<Guid, TEntity> where TEntity : GuidEntity { }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    public class CachedReference<TId, TEntity> : ICachedReference where TEntity : Entity<TId> where TId : struct
    {
        TEntity Value;
        TId? Id;

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public async Task<TEntity> Get(TId? id)
        {
            if (!Id.Equals(id)) Value = null; // Different ID from the cache.
            Id = id;

            if (Value == null)
            {
                if (id == null) return null;

                var result = await Entity.Database.Get<TEntity>(id.ToString());

                if (!Entity.Database.AnyOpenTransaction())
                {
                    Value = result;
                    Value.RegisterCachedCopy(this);
                }
                else return result;
            }

            return Value;
        }

        void Bind(TEntity entity)
        {
            Id = entity?.ID ?? throw new ArgumentNullException(nameof(entity));
            Value = entity;

            if (!Entity.Database.AnyOpenTransaction())
                Value.RegisterCachedCopy(this);
        }

        void ICachedReference.Invalidate() => Value = null;
    }
}