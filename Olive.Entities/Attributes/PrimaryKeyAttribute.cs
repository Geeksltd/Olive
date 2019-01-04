using System;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, it marks it as the primary key of that class.
    /// This is intended to be used by object relational mapping (ORM) tools.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Determines if a given property is primary key.
        /// </summary>
        public static bool IsPrimaryKey(PropertyInfo property)
        {
            return property.GetCustomAttribute<PrimaryKeyAttribute>(inherit: false) != null;
        }
    }
}