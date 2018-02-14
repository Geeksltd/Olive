using System;
using System.ComponentModel;

namespace Olive.Entities
{
    /// <summary>Represents a base Entity with ID of type Guid.</summary>
    public abstract class GuidEntity : Entity<Guid>, IEntity<Guid>
    {
        bool IsIdLoaded; // For performance, this is used instead of Nullable<Guid>
        Guid id;

        /// <summary>
        /// Gets a unique Identifier for this instance. In the database, this will be the primary key of this object.
        /// </summary>
        public override Guid ID
        {
            get
            {
                if (IsIdLoaded) return id;
                else
                {
                    id = Guid.NewGuid();
                    IsIdLoaded = true;
                    return id;
                }
            }
            set
            {
                id = value;
                IsIdLoaded = true;
            }
        }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// If you need to update an instance loaded from the database, you must create a Clone of it before applying any changes.
        /// Otherwise you will be editing the "live" instance from the cache, that is used by all other threads!
        /// </summary>
        public override IEntity Clone()
        {
            var result = base.Clone() as GuidEntity;

            // This is needed to avoid the problem caused by lazy loading of ID value.
            result.ID = ID;

            return result;
        }

        public static bool operator !=(GuidEntity entity, Guid? id) => entity?.ID != id;

        public static bool operator ==(GuidEntity entity, Guid? id) => entity?.ID == id;

        public static bool operator !=(GuidEntity entity, Guid id) => entity?.ID != id;

        public static bool operator ==(GuidEntity entity, Guid id) => entity?.ID == id;

        public static bool operator !=(Guid? id, GuidEntity entity) => entity?.ID != id;

        public static bool operator ==(Guid? id, GuidEntity entity) => entity?.ID == id;

        public static bool operator !=(Guid id, GuidEntity entity) => entity?.ID != id;

        public static bool operator ==(Guid id, GuidEntity entity) => entity?.ID == id;

        public override bool Equals(Entity other) => ID == (other as GuidEntity)?.ID;

        public override bool Equals(object other) => ID == (other as GuidEntity)?.ID;

        public override int GetHashCode() => ID.GetHashCode();
    }
}