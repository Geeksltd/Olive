using Olive;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveEntitiesExtensions
    {
        /// <summary>
        /// Determines if this item is in a specified list of specified items.
        /// </summary>
        public static bool IsAnyOf<T>(this T item, params T[] options) where T : IEntity
        {
            if (item == null) return options.Contains(default(T));

            return options.Contains(item);
        }

        /// <summary>
        /// Determines if this item is in a specified list of specified items.
        /// </summary>
        public static bool IsAnyOf<T>(this T item, IEnumerable<T> options) where T : IEntity
        {
            return options.Contains(item);
        }

        /// <summary>
        /// Determines if this item is none of a list of specified items.
        /// </summary>
        public static bool IsNoneOf<T>(this T item, params T[] options) where T : IEntity
        {
            if (item == null) return !options.Contains(default(T));

            return !options.Contains(item);
        }

        /// <summary>
        /// Determines if this item is none of a list of specified items.
        /// </summary>
        public static bool IsNoneOf<T>(this T item, IEnumerable<T> options) where T : IEntity
        {
            if (item == null) return !options.Contains(default(T));

            return !options.Contains(item);
        }

        /// <summary>
        /// Clones all items of this collection.
        /// </summary>
        public static List<T> CloneAll<T>(this IEnumerable<T> list) where T : IEntity
        {
            return list.Select(i => i.Clone()).Cast<T>().ToList();
        }

        /// <summary>
        /// Gets the id of this entity.
        /// </summary>
        public static string GetFullIdentifierString(this IEntity entity)
        {
            if (entity == null) return null;

            return entity.GetType().GetRootEntityType().FullName + "/" + entity.GetId();
        }

        /// <summary>
        /// Validates all entities in this collection.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entities">The entities.</param>
        public static Task ValidateAll<T>(this IEnumerable<T> entities) where T : Entity
        {
            return Task.WhenAll(entities.Select(x => x.Validate()));
        }

        /// <summary>
        /// Returns this Entity only if the given predicate evaluates to true and this is not null.
        /// </summary>        
        public static T OnlyWhen<T>(this T entity, Func<T, bool> criteria) where T : Entity
        {
            return entity != null && criteria(entity) ? entity : null;
        }

        /// <summary>
        /// Returns all entity Guid IDs for this collection.
        /// </summary>
        public static IEnumerable<TId> IDs<TId>(this IEnumerable<IEntity<TId>> entities)
        {
            return entities.Select(entity => entity.ID);
        }
    }
}