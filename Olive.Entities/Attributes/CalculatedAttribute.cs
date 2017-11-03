using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates that such property does not exist in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CalculatedAttribute : Attribute
    {
        static object DetectAndCacheShouldBeStaticMethod = new object();

        static Dictionary<PropertyInfo, bool> Cache = new Dictionary<PropertyInfo, bool>();

        /// <summary>
        /// Determines if a given property is calculated.
        /// </summary>
        public static bool IsCalculated(PropertyInfo property)
        {
            if (Cache.TryGetValue(property, out bool result)) return result;

            return DetectAndCache(property);
        }

        static bool DetectAndCache(PropertyInfo property)
        {
            lock (DetectAndCacheShouldBeStaticMethod)
            {
                var result = property.IsDefined(typeof(CalculatedAttribute), inherit: true);

                try { return Cache[property] = result; }
                catch { return result; }
            }
        }
    }
}