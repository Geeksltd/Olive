using System;
using System.Reflection;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IdByDatabaseAttribute : Attribute
    {
        public static bool IsIdAssignedByDatabase(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<IdByDatabaseAttribute>(inherit: true) != null;
        }
    }
}
