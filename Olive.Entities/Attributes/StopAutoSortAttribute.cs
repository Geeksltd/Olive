using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StopAutoSortAttribute : Attribute
    {
        static readonly ConcurrentDictionary<Type, bool> Cache = 
            new ConcurrentDictionary<Type, bool>();

        public static bool HasAttribute(Type type)
        {
            return Cache.GetOrAdd(
                type, 
                t => t.GetCustomAttributes<StopAutoSortAttribute>(true).Any());
        }
    }
}
