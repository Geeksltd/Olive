using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CustomPropertyAccessorAttribute : Attribute
    {
        readonly string ClassName;
        readonly bool NestedClass;

        /// <summary>
        /// Provide a custom logic for the property accessor.
        /// </summary>
        /// <param name="className">It should be the full name of the type unless it is a nested class</param>
        /// /// <param name="nestedClass">Specify the class name as the nested class of declaring type.</param>
        public CustomPropertyAccessorAttribute(string className, bool nestedClass = false)
        {
            ClassName = className;
            NestedClass = nestedClass;
        }

        public static string GetClassName(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CustomPropertyAccessorAttribute>(inherit: false);

            if (attribute == null) return null;

            return attribute.NestedClass ? $"{property.DeclaringType.FullName}+{attribute.ClassName}" : attribute.ClassName;
        }
    }
}
