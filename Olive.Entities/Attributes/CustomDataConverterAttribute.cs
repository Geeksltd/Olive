using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CustomDataConverterAttribute : Attribute
    {
        readonly string ClassName;

        /// <summary>
        /// Provide a custom converter from database data type the the property data type.
        /// </summary>
        /// <param name="className">It should be the full name of the type.</param>
        public CustomDataConverterAttribute(string className) => ClassName = className;

        public static string GetClassName(PropertyInfo property) =>
            property.GetCustomAttribute<CustomDataConverterAttribute>(inherit: false)?.ClassName;
    }
}
