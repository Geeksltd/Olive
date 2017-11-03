using System;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates that such property is AutoNumber (or Identity in SQL Server).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AutoNumberAttribute : Attribute
    {
        /// <summary>
        /// Determines if a given property is auto number.
        /// </summary>
        public static bool IsAutoNumber(PropertyInfo property)
        {
            return property.GetCustomAttribute<AutoNumberAttribute>(inherit: false) != null;
        }
    }
}