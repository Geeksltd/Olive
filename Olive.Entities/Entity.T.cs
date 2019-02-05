namespace Olive.Entities
{
    using Newtonsoft.Json;
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class Entity<T> : Entity, IEntity<T>, IOriginalIdHolder
    {
        /// <summary>Gets or sets the ID of this object.</summary>
        [EscapeGCop("It is the base class")]
        public virtual T ID { get; set; }

        /// <summary>
        /// Gets the original id of this type as it was in the database.
        /// </summary>
        [EscapeGCop("It is the base class")]
        [XmlIgnore, JsonIgnore, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual T OriginalId { get; internal set; }

        void IOriginalIdHolder.SetOriginalId() => OriginalId = ID;

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