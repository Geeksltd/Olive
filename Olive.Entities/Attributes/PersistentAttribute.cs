namespace Olive.Entities
{
    /// <summary>
    /// When applied to a class, defines its Application data accessor type.
    /// </summary>
    public sealed class PersistentAttribute : Attribute
    {
        bool IsPersistent;
        public PersistentAttribute(bool isPersistent)
        {
            IsPersistent = isPersistent;
        }

        internal static bool IsTypePersistent(Type type)
        {
            if (type.GetInterface(typeof(IEntity).FullName) == null)
                return false;

            if (type.IsDefined(typeof(PersistentAttribute), inherit: true))
            {
                foreach (PersistentAttribute attribute in type.GetCustomAttributes(typeof(PersistentAttribute), inherit: true))
                    if (attribute.IsPersistent == false) return false;

            }

            // Default unconfigured value is true:
            return true;
        }
    }
}