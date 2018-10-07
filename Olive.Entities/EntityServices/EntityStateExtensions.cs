using System;

namespace Olive.Entities
{
    public static class EntityStateExtensions
    {
        /// <summary>
        /// Determines whether the specified record is immutable, or closed for changes.        
        /// An object marked as immutable is shared in the application cache. Therefore it must not be changed.        
        /// </summary>
        public static bool IsImmutable(this EntityServices @this, IEntity entity)
        {
            return ((Entity)entity).IsImmutable && !entity.IsNew;
        }

        /// <summary>
        /// Marks the specified object as immutable.
        /// </summary>
        public static void MarkImmutable(this EntityServices @this, IEntity entity)
        {
            ((Entity)entity).IsImmutable = true;
        }

        /// <summary>
        /// Sets the state of an entity instance to saved.
        /// </summary>
        public static void SetSaved(this EntityServices @this, IEntity entity, bool saved = true)
        {
            (entity as Entity).IsNew = !saved;

            entity.GetType().GetProperty("OriginalId").SetValue(entity, entity.GetId());
        }

        /// <summary>
        /// Creates a new clone of an entity. This will work in a polymorphic way.
        /// </summary>        
        public static T CloneAsNew<T>(this EntityServices @this, T entity, Action<T> changes = null) where T : Entity
        {
            var result = (T)entity.Clone();
            result.IsNew = true;

            if (result is GuidEntity) (result as GuidEntity).ID = Guid.NewGuid();
            // TODO: the following line need to be reviewed and fixed.
            // if (result is IntEntity) (result as IntEntity).ID = IntEntity.NewIdGenerator(result.GetType());

            // Setting the value of AutoNumber properties to zero
            foreach (var propertyInfo in result.GetType().GetProperties())
                if (AutoNumberAttribute.IsAutoNumber(propertyInfo))
                    propertyInfo.SetValue(result, 0);

            result.Initialize();

            // Re attach Documents:
            changes?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Sets the ID of an object explicitly.
        /// </summary>
        public static void ResetOriginalId<T>(this EntityServices @this, IEntity<T> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ((dynamic)entity).OriginalId = entity.ID;
        }

        public static void SetSaved<T>(this EntityServices @this, IEntity<T> entity, T id)
        {
            ((dynamic)entity).IsNew = false;

            entity.ID = id;
            @this.ResetOriginalId(entity);
        }
    }
}