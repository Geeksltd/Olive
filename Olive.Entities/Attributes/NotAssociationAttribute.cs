using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates that such property is not an association. 
    /// It uses when you need a property ending with `Id` but it is not an entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotAssociationAttribute : Attribute
    {
        /// <summary>
        /// Determines if a given property is mark by this attribute.
        /// </summary>
        public static bool Marked(PropertyInfo property)
        {
            return property.GetCustomAttribute<NotAssociationAttribute>(inherit: true) != null;
        }
    }
}
