using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Olive.Entities
{
    /// <summary>Entity, a persistent object in the application.</summary>
    [Serializable]
    public abstract class Entity : IEntity
    {
        static Dictionary<Type, PropertyInfo[]> PrimitiveProperties = new Dictionary<Type, PropertyInfo[]>();
        static object PrimitivePropertiesSyncLock = new object();

        object CachedCopiesLock = new object();

        [NonSerialized]
        internal List<ICachedReference> CachedCopies;
        internal bool IsImmutable;

        [NonSerialized, XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public Entity _ClonedFrom;

        public static readonly EntityServices Services = new EntityServices();

        /// <summary>
        /// Base constructor (called implicitly in all typed entity classes) to initialize an object.
        /// </summary>
        protected Entity()
        {
            IsNew = true;
            IsImmutable = true;
            Initialize();
        }

        /// <summary>
        /// Gets the id of this entity.
        /// </summary>
        public abstract object GetId();

        public override int GetHashCode() => GetId().GetHashCode();

        #region CachedCopies

        internal void RegisterCachedCopy(ICachedReference cachedCopy)
        {
            if (cachedCopy == null) return;
            lock (CachedCopiesLock)
            {
                if (CachedCopies == null) CachedCopies = new List<ICachedReference>();
                CachedCopies.Add(cachedCopy);
            }
        }

        /// <summary>
        /// Invalidates its cached references.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void InvalidateCachedReferences()
        {
            lock (CachedCopiesLock)
            {
                if (CachedCopies == null) CachedCopies = new List<ICachedReference>();
                else foreach (var c in CachedCopies) c.Invalidate();
            }

            _ClonedFrom?.InvalidateCachedReferences();
        }

        #endregion

        /// <summary>
        /// Determines whether this is a newly created instace. This value will be True for new objects, and False for anything loaded from the database.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public virtual bool IsNew { get; internal set; }

        /// <summary>
        /// Determines whether this instance is "soft-deleted".
        /// </summary>
        [XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool IsMarkedSoftDeleted { get; internal set; }

        /// <summary>
        /// Determines whether this object is already cloned and updated in the database without this instance being updated.
        /// </summary>
        [NonSerialized, XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsStale;

        /// <summary>
        /// Initializes this instance.
        /// This can be overridden in the business entity types to provide "construction" logic.
        /// </summary>
        protected internal virtual void Initialize() { }

        #region ToString(format)

        /// <summary>
        /// Gets the primitive properties of this tye.
        /// </summary>
        PropertyInfo[] GetPrimitiveProperties()
        {
            var myType = GetType();
            if (PrimitiveProperties.ContainsKey(myType))
            {
                // Already cached:
                return PrimitiveProperties[myType];
            }
            else
            {
                lock (PrimitivePropertiesSyncLock)
                {
                    if (PrimitiveProperties.ContainsKey(myType))
                        return PrimitiveProperties[myType];
                    var result = ExtractPrimitiveProperties(myType);
                    PrimitiveProperties.Add(myType, result);
                    return result;
                }
            }
        }

        /// <summary>
        /// Extracts the primitive properties of a specified type.
        /// </summary>
        static PropertyInfo[] ExtractPrimitiveProperties(Type type)
        {
            var result = new List<PropertyInfo>();
            var primitiveTypes = new[] { typeof(string), typeof(int), typeof(int?), typeof(double), typeof(double?), typeof(DateTime), typeof(DateTime?) };
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead).Where(p => primitiveTypes.Contains(p.PropertyType)))
            {
                if (p.Name == nameof(IsNew)) continue;
                if (p.PropertyType.Implements<IEntity>()) continue;
                if (CalculatedAttribute.IsCalculated(p)) continue;
                result.Add(p);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns a string that contains all primitive properties of this instance.
        /// This should be used normally in "full text search".
        /// </summary>
        public virtual string ToString(string format)
        {
            if (format == "F")
            {
                var r = new StringBuilder();
                foreach (var p in GetPrimitiveProperties())
                {
                    try
                    {
                        r.Append(p.GetValue(this)?.ToString() + " ");
                    }
                    catch
                    {
                        // We don't want this method to throw an exception even if some properties cannot be read.
                        // No logging is needed
                    }
                }

                return r.ToString();
            }
            else
                return ToString();
        }

        #endregion

        /// <summary>
        /// Validates the data for the properties of the current instance.
        /// It throws a ValidationException if an error is detected
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual Task ValidateProperties() => Task.CompletedTask;

        /// <summary>
        /// Validates this instance to ensure it can be saved in a data repository.
        /// If this finds an issue, it throws a ValidationException for that.        
        /// This calls ValidateProperties(). Override this method to provide custom validation logic in a type.
        /// </summary>
        public virtual Task Validate() => ValidateProperties();

        /// <summary>
        /// This even is raised just after this instance is loaded from the database.
        /// </summary>        
        public event AwaitableEventHandler Loaded;
        protected internal virtual Task OnLoaded() => Loaded.Raise();

        /// <summary>
        /// This event is raised just before this instance is saved in the data repository.
        /// </summary>
        public event AwaitableEventHandler<CancelEventArgs> Saving;
        protected internal virtual Task OnSaving(CancelEventArgs e) => Saving.Raise(e);

        /// <summary>
        /// This is raised just before the object is being Validated.
        /// It will automatically be called in Database.Save() method before calling the Validate() method.
        /// Use this to do any last-minute object modifications, such as initializing complex values.
        /// </summary>
        public event AwaitableEventHandler Validating;
        protected internal virtual Task OnValidating(EventArgs e) => Validating.Raise();

        /// <summary>
        /// This event is raised after this instance is saved in the database.
        /// </summary>
        public event AwaitableEventHandler<SaveEventArgs> Saved;
        protected internal virtual async Task OnSaved(SaveEventArgs e)
        {
            InvalidateCachedReferences();
            await Saved.Raise(e);
            await GlobalEntityEvents.OnInstanceSaved(new GlobalSaveEventArgs(this, e.Mode));
            InvalidateCachedReferences();
        }

        /// <summary>
        /// This event is raised just before this instance is deleted from the database.
        /// </summary>
        public event AwaitableEventHandler<CancelEventArgs> Deleting;
        protected internal virtual Task OnDeleting(CancelEventArgs e) => Deleting.Raise(e);

        /// <summary>
        /// This event is raised just after this instance is deleted from the database.
        /// </summary> 
        public event AwaitableEventHandler Deleted;
        protected internal virtual async Task OnDeleted(EventArgs e)
        {
            InvalidateCachedReferences();
            await Deleted.Raise();
            await GlobalEntityEvents.OnInstanceDeleted(new GlobalDeleteEventArgs(this));
            InvalidateCachedReferences();
        }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// If you need to update an instance loaded from the database, you must create a Clone of it before applying any changes.
        /// Otherwise you will be editing the "live" instance from the cache, that is used by all other threads!
        /// </summary>
        public virtual IEntity Clone()
        {
            var result = (Entity)MemberwiseClone();
            result.IsImmutable = false;

            result._ClonedFrom = this;
            return result;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.        
        /// </summary>
        public override bool Equals(object @object) => Equals(@object as Entity);

        /// <summary>Determines whether the specified object is equal to this instance. </summary>
        public abstract bool Equals(Entity @object);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(Entity left, object right)
        {
            if (right == null) return left == null;

            if (right is Entity rightEntity)
                return left == rightEntity;

            if (right is Task)
                throw new Exception("You cannot compare an entity object with a Task object. Await the task before comparing.");

            return false;
        }

        public static bool operator !=(Entity left, object right) => !(left == right);

        public static bool operator ==(Entity left, Entity right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right) => !(left == right);

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name = "other">An object to compare with this instance.</param>
        public virtual int CompareTo(object other)
        {
            if (other == null) return 1;
            else return string.Compare(ToString(), other.ToString(), ignoreCase: true);
        }

        /// <summary>
        /// Gets the currently injected Datbase service.
        /// </summary>
        protected static IDatabase Database => Context.Current.Database();
    }
}