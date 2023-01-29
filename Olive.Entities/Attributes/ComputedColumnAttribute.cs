using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ComputedColumnAttribute : Attribute
    {
        static object DetectAndCacheShouldBeStaticMethod = new object();

        static Dictionary<PropertyInfo, bool> Cache = new Dictionary<PropertyInfo, bool>();

        /// <summary>
        /// Determines if a given property is calculated.
        /// </summary>
        public static bool IsComputedColumn(PropertyInfo property)
        {
            if (Cache.TryGetValue(property, out var result)) return result;

            return DetectAndCache(property);
        }

        static bool DetectAndCache(PropertyInfo property)
        {
            lock (DetectAndCacheShouldBeStaticMethod)
            {
                var result = property.IsDefined(typeof(ComputedColumnAttribute), inherit: true);

                try { return Cache[property] = result; }
                catch
                {
                    // No logging is needed
                    return result;
                }
            }
        }
    }
}
