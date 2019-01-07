using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities
{
    /// <summary>
    /// Finds an entity with unknown type from its given id.
    /// </summary>
    public static class EntityFinder
    {
        static readonly ConcurrentDictionary<Type, IEnumerable<Type>> PossibleTypesCache = new ConcurrentDictionary<Type, IEnumerable<Type>>();

        // static readonly ConcurrentDictionary<object, Type> EntityTypesCache = new ConcurrentDictionary<object, Type>();

        public static IEnumerable<Type> FindPossibleTypes(Type baseType, bool mustFind) =>
            PossibleTypesCache.GetOrAdd(baseType, t => SearchForPossibleTypes(t, mustFind));

        /// <summary>
        /// Gets the runtime type from the currently loaded assemblies.
        /// </summary>
        /// <param name="typeFullName">The type name (including namespace, but excluding assembly).</param>
        public static Type GetEntityType(string typeFullName) =>
            GetDomainEntityTypes().FirstOrDefault(x => x.FullName == typeFullName);

        public static IEnumerable<Type> GetDomainEntityTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                 .Where(a => a.References(typeof(Entity).Assembly))
                 .SelectMany(a => a.GetExportedTypes())
                 .Where(t => t.Implements<IEntity>())
                 .Except(t => t.Name.StartsWith("App_Code"));
        }

        public static IEnumerable<Type> SearchForPossibleTypes(Type baseType, bool mustFind)
        {
            IEnumerable<Type> result;

            if (baseType == null || baseType == typeof(Entity))
                result = GetDomainEntityTypes();

            else if (baseType.IsInterface)
                result = AppDomain.CurrentDomain.FindImplementers(baseType);

            else
                result = baseType.Assembly.GetExportedTypes().Where(t => t.GetParentTypes().Contains(baseType)).Union(new[] { baseType });

            result = result
                // Not transient objects:
                .Where(t => !TransientEntityAttribute.IsTransient(t))
                // No abstract or interface:
                .Where(t => !t.IsAbstract && !t.IsInterface)
                // Unless the type is marked non-persistent:
                .Where(t => PersistentAttribute.IsTypePersistent(t))
                // Leaf nodes first (most concrete):
                .OrderByDescending(t => t.GetParentTypes().Count());

            if (result.None())
            {
                if (baseType != null && mustFind)
                    throw new ArgumentException($"No type in the current application domain can be the implementation of the type {baseType.FullName}.");

                else if (mustFind)
                    throw new ArgumentException("No type in the current application domain implements Entity.");
            }

            return result;
        }

        // /// <summary>
        // /// Finds the actual type of an object with the specified ID which inherits from a specified base type.
        // /// </summary>
        // public static IEntity Find(object entityId, Type baseType)
        // {
        //     var key = baseType

        //     return EntityTypesCache.GetOrAdd(
        // }

        // static IEntity DiscoverType(object entityId, Type baseType)
        // {
        //     if (baseType != null && !baseType.Implements<IEntity>())
        //         throw new ArgumentException(baseType.FullName + " is not acceptable as a valid entity base type.");

        //     if (baseType == null && !baseType.IsA<GuidEntity>())
        //     {
        //         throw new ArgumentException("Only Guid-based entities can be retrieved from the database by their ID only.");
        //     }

        //     var type = FromCache(entityId);
        //     if (type == null)
        //     {
        //         // Find possible types:
        //         var possibleTypes = FindPossibleTypes(baseType, true);

        //         if (possibleTypes.Count() == 1)
        //         {
        //             type = possibleTypes.Single();
        //             Cache(entityId, type);
        //             return Database.GetConcrete(entityId, type);
        //         }
        //         else
        //         {
        //             Exception firstError = null;

        //             foreach (var candidateType in possibleTypes)
        //             {
        //                 Entity result;
        //                 try { result = Database.GetConcrete(entityId, candidateType); }
        //                 catch (Exception ex) { if (firstError == null) firstError = ex; continue; }

        //                 if (result != null)
        //                 {
        //                     Cache(entityId, result.GetType());
        //                     return result;
        //                 }
        //             }

        //             if (firstError != null) throw firstError;
        //         }
        //     }
        //     else return Database.GetConcrete(entityId, type);

        //     var error = "Could not find the type of the entity with the ID of " + entityId;
        //     if (baseType != null && baseType != typeof(Entity))
        //         error += ", considered to be a an implementation of " + baseType.FullName;

        //     throw new ArgumentException(error + ".");
        // }

        public static void ResetCache() => PossibleTypesCache.Clear(); //EntityTypesCache.Clear();
    }
}
