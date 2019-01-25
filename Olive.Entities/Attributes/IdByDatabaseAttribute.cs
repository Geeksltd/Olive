using System;
using System.Reflection;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IdByDatabaseAttribute : Attribute
    {
        public static bool IsIdAssignedByDatabase(Type type, bool inherit = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<IdByDatabaseAttribute>(inherit) != null;
        }
    }
}
