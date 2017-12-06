namespace Olive.Entities
{
    /// <summary>
    /// Provides services for Entity objects.
    /// </summary>
    public static class EntityManager
    {
        /// <summary>
        /// Determines whether the specified record is immutable, or closed for changes.        
        /// An object marked as immutable is shared in the application cache. Therefore it must not be changed.        
        /// </summary>
        public static bool IsImmutable(IEntity entity)
        {
            var item = entity as Entity;

            if (item == null)
                throw new ArgumentNullException("entity must be a non-null instance inheriting from Entity.");

            return item.IsImmutable && !entity.IsNew;
        }

        /// <summary>
        /// Marks the specified object as immutable.
        /// </summary>
        public static void MarkImmutable(IEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            (entity as Entity).IsImmutable = true;
        }

        #region Entity static events
        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is saved in the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<CancelEventArgs> InstanceSaving = new AsyncEvent<CancelEventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type after "any" object is saved in the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<SaveEventArgs> InstanceSaved = new AsyncEvent<SaveEventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is deleted from the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<CancelEventArgs> InstanceDeleting = new AsyncEvent<CancelEventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type before "any" object is validated.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// This will be called as the first line of the base Entity's OnValidating method.
        /// </summary>
        public readonly static AsyncEvent<EventArgs> InstanceValidating = new AsyncEvent<EventArgs>();

        /// <summary>
        /// This event is raised for the whole Entity type after "any" object is deleted from the database.
        /// You can handle this to provide global functionality/event handling scenarios.
        /// </summary>
        public readonly static AsyncEvent<EventArgs> InstanceDeleted = new AsyncEvent<EventArgs>();
        #endregion

        #region Raise events

        internal static Task RaiseStaticOnSaved(SaveEventArgs args) =>
            InstanceSaved.Raise(args);

        internal static Task RaiseStaticOnDeleted(EventArgs args) =>
            InstanceDeleted.Raise(args);

        public static async Task RaiseOnDeleting(IEntity record, CancelEventArgs args)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await InstanceDeleting.Raise(args);

            if (args.Cancel) return;

            await (record as Entity).OnDeleting(args);
        }

        public static async Task RaiseOnValidating(IEntity record, EventArgs args)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await InstanceValidating.Raise(args);

            await (record as Entity).OnValidating(args);
        }

        public static async Task RaiseOnDeleted(IEntity record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await (record as Entity).OnDeleted(EventArgs.Empty);
        }

        public static async Task RaiseOnLoaded(IEntity record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await (record as Entity).OnLoaded();
        }

        public static async Task RaiseOnSaving(IEntity record, CancelEventArgs e)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await InstanceSaving.Raise(e);
            if (e.Cancel) return;

            await (record as Entity).OnSaving(e);
        }

        public static async Task RaiseOnSaved(IEntity record, SaveEventArgs e)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await (record as Entity).OnSaved(e);
        }

        #endregion

        /// <summary>
        /// Sets the state of an entity instance to saved.
        /// </summary>
        public static void SetSaved(IEntity entity, bool saved = true)
        {
            (entity as Entity).IsNew = !saved;

            entity.GetType().GetProperty("OriginalId").SetValue(entity, entity.GetId());
        }

        /// <summary>
        /// Creates a new clone of an entity. This will work in a polymorphic way.
        /// </summary>        
        public static T CloneAsNew<T>(T entity) where T : Entity => CloneAsNew<T>(entity, null);

        /// <summary>
        /// Creates a new clone of an entity. This will work in a polymorphic way.
        /// </summary>        
        public static T CloneAsNew<T>(T entity, Action<T> changes) where T : Entity
        {
            var result = (T)entity.Clone();
            result.IsNew = true;

            if (result is GuidEntity) (result as GuidEntity).ID = GuidEntity.NewGuidGenerator(result.GetType());
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
        public static void RestsetOriginalId<T>(IEntity<T> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ((dynamic)entity).OriginalId = entity.ID;
        }

        public static void SetSaved<T>(IEntity<T> entity, T id)
        {
            ((dynamic)entity).IsNew = false;

            entity.ID = id;
            RestsetOriginalId(entity);
        }

        /// <summary>
        /// Read the value of a specified property from a specified object.
        /// </summary>
        public static object ReadProperty(object @object, string propertyName)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            var property = FindProperty(@object.GetType(), propertyName);

            try
            {
                return property.GetValue(@object, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not read the value of the property {propertyName } from the given {@object.GetType().FullName} object.", ex);
            }
        }

        public static PropertyInfo FindProperty(Type type, string propertyName)
        {
            if (propertyName.IsEmpty()) throw new ArgumentNullException(nameof(propertyName));

            var result = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (result == null) // Try inherited properties.
                result = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            if (result == null) throw new ArgumentException($"{type} does not have a property named {propertyName}");

            return result;
        }

        public static void WriteProperty(object @object, string propertyName, object value)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            var property = FindProperty(@object.GetType(), propertyName);

            try
            {
                property.SetValue(@object, value, null);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Could not set the value of the property {propertyName} from the given {@object.GetType().FullName} object.", ex);
            }
        }

        public static bool IsSoftDeleted(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return entity.IsMarkedSoftDeleted;
        }

        public static void MarkSoftDeleted(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsMarkedSoftDeleted = true;
        }

        public static void UnMarkSoftDeleted(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsMarkedSoftDeleted = false;
        }
    }
}