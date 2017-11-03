using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CacheDependentAttribute : Attribute
    {
        /// <summary>
        /// Gets the dependent type.
        /// </summary>
        public Type DependentType { get; private set; }

        /// <summary>
        /// Creates a new CacheDependantAttribute instance.
        /// </summary>
        public CacheDependentAttribute(Type dependentType) =>
            DependentType = dependentType ?? throw new ArgumentNullException(nameof(dependentType));

        static ConcurrentDictionary<Type, Type[]> Cache = new ConcurrentDictionary<Type, Type[]>();

        /// <summary>
        /// Gets a list of types that depend on a given entity.
        /// </summary>
        public static IEnumerable<Type> GetDependentTypes(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return Cache.GetOrAdd(entityType, FindDependentTypes);
        }

        /// <summary>
        /// Finds a list of types that depend on a given entity.
        /// </summary>
        static Type[] FindDependentTypes(Type entityType)
        {
            return (from type in entityType.Assembly.GetTypes()
                    from p in type.GetProperties()
                    let att = p.GetCustomAttribute<CacheDependentAttribute>()
                    where att != null && att.DependentType.IsAssignableFrom(entityType)
                    select type).Distinct().ToArray();
        }
    }
}