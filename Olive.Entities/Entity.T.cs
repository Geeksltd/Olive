namespace Olive.Entities
{
    using Newtonsoft.Json;
    using System.Xml.Serialization;

    public class Entity<T> : Entity, IEntity<T>
    {
        /// <summary>
        /// Gets or sets the ID of this object.
        /// </summary>
        public virtual T ID { get; set; }

        /// <summary>
        /// Gets the original id of this type as it was in the database.
        /// </summary>
        [XmlIgnore, JsonIgnore, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual T OriginalId { get; internal set; }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => ID.GetHashCode();

        /// <summary>
        /// Determines whether this instance is equal to another specified instance.
        /// </summary>
        public override bool Equals(Entity other)
        {
            if (ReferenceEquals(this, other)) return true;

            var typed = other as Entity<T>;

            if (typed is null) return false;

            if (GetType() != typed.GetType()) return false;

            return ID.Equals(typed.ID);
        }

        /// <summary>
        /// Gets the ID of this object.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override object GetId() => ID;
    }
}