using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides extension methods for loading the associations of a record, so that reading those association
    /// properties later is served from memory rather than loading them synchronously.
    /// <para/>
    /// Unlike Include() on a query, the associations are served from the cache when possible.
    /// Only the specified associations are loaded, and each stays loaded until its record is changed or deleted,
    /// after which reading it loads it again.
    /// </summary>
    public static class DatabaseIncludeExtensions
    {
        /// <summary>
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of the
        /// record that this task returns, if any, so that reading them is not a synchronous database call.
        /// For example: await Database.FindById&lt;Content&gt;(id).Including(x => x.Module).
        /// </summary>
        public static async Task<T> Including<T>(this Task<T> @this,
            params Expression<Func<T, object>>[] associations) where T : IEntity
        {
            var result = await @this;
            await Context.Current.Database().IncludeAssociations(result, associations);
            return result;
        }

        /// <summary>
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of a
        /// record already in hand, so that reading them is not a synchronous database call.
        /// </summary>
        public static Task IncludeAssociations<T>(this IDatabase @this, T entity,
            params Expression<Func<T, object>>[] associations) where T : IEntity =>
            @this.IncludeAssociations(entity, associations.Select(x => x.GetPropertyPath()).ToArray());

        /// <summary>
        /// Loads the specified associations (e.g. "Module", or "Discussion.Module" for a nested one) of a record
        /// already in hand, so that reading them is not a synchronous database call.
        /// An association whose ID is empty, or which is not a cached reference, is skipped.
        /// </summary>
        public static async Task IncludeAssociations(this IDatabase @this, IEntity entity, params string[] associations)
        {
            if (entity == null) return;

            foreach (var path in associations)
                await @this.IncludeAssociation(entity, path.Split('.'));
        }

        static async Task IncludeAssociation(this IDatabase @this, IEntity entity, string[] path)
        {
            var association = entity.GetType().GetProperty(path[0]) ??
                throw new ArgumentException($"{entity.GetType().Name} has no property named {path[0]}.");

            // The same members that Include() binds through.
            var cachedField = association.DeclaringType.GetField("cached" + association.Name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            var idProperty = association.DeclaringType.GetProperty(association.Name + "Id");

            // Skipping it would leave reading it to load it synchronously, which is what this is meant to prevent.
            if (cachedField == null || idProperty == null)
                throw new ArgumentException($"{association.DeclaringType.Name}.{association.Name} is not an " +
                    $"association backed by a cached{association.Name} field and a {association.Name}Id property.");

            var id = idProperty.GetValue(entity);
            if (id.ToStringOrEmpty().IsEmpty()) return; // Reading it returns null without loading anything.

            var associated = @this is Database database ?
                await database.FindById(id, association.PropertyType) :
                await @this.GetOrDefault(id, association.PropertyType);
            if (associated == null) return;

            // Bound to the ID it is read by, rather than its own, which may differ in case for a string ID.
            var cachedRef = cachedField.GetValue(entity);
            cachedRef.GetType().GetMethod("BindTo", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(cachedRef, new[] { id, associated });

            if (path.Length > 1) await @this.IncludeAssociation(associated, path.Skip(1).ToArray());
        }
    }
}
