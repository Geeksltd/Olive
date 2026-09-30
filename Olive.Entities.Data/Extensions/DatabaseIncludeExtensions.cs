using System;
using System.Collections.Generic;
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
        /// The most IDs loaded in one query. They are written into the SQL, rather than as parameters.
        /// </summary>
        const int IdListBatchSize = 500;

        /// <summary>
        /// Whether a provider loads the records of a list of IDs in one query. The SQL providers do. Others may not,
        /// e.g. DynamoDB would read the list as a single key, so their records are loaded by their IDs.
        /// </summary>
        internal static Func<IDataProvider, bool> CanLoadIdList = x => x is DataProvider;

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
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of
        /// every record that this task returns, so that reading them is not a synchronous database call.
        /// A record shared by several of them is loaded once.
        /// For example: await Database.GetList&lt;Content&gt;().Including(x => x.Module).
        /// </summary>
        public static async Task<T[]> Including<T>(this Task<T[]> @this,
            params Expression<Func<T, object>>[] associations) where T : IEntity
        {
            var result = await @this;
            await Context.Current.Database().IncludeAssociations(result, associations);
            return result;
        }

        /// <summary>
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of
        /// every record that this task returns, so that reading them is not a synchronous database call.
        /// A record shared by several of them is loaded once.
        /// </summary>
        public static async Task<List<T>> Including<T>(this Task<List<T>> @this,
            params Expression<Func<T, object>>[] associations) where T : IEntity
        {
            var result = await @this;
            await Context.Current.Database().IncludeAssociations(result, associations);
            return result;
        }

        /// <summary>
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of
        /// every record that this task returns, so that reading them is not a synchronous database call.
        /// A record shared by several of them is loaded once.
        /// </summary>
        public static async Task<IList<T>> Including<T>(this Task<IList<T>> @this,
            params Expression<Func<T, object>>[] associations) where T : IEntity
        {
            var result = await @this;
            await Context.Current.Database().IncludeAssociations(result, associations);
            return result;
        }

        /// <summary>
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of
        /// every record that this task returns, so that reading them is not a synchronous database call.
        /// A record shared by several of them is loaded once.
        /// A deferred sequence is enumerated once, and the records it returns are the ones returned.
        /// </summary>
        public static async Task<IEnumerable<T>> Including<T>(this Task<IEnumerable<T>> @this,
            params Expression<Func<T, object>>[] associations) where T : IEntity
        {
            // Enumerating it again could evaluate it again, returning records that are not bound.
            var result = (await @this)?.ToArray();
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
        /// Loads the specified associations (e.g. x => x.Module, or x => x.Discussion.Module for a nested one) of
        /// records already in hand, so that reading them is not a synchronous database call.
        /// A record shared by several of them is loaded once.
        /// </summary>
        public static Task IncludeAssociations<T>(this IDatabase @this, IEnumerable<T> entities,
            params Expression<Func<T, object>>[] associations) where T : IEntity =>
            @this.IncludeAssociations(entities?.Cast<IEntity>(), associations.Select(x => x.GetPropertyPath()).ToArray());

        /// <summary>
        /// Loads the specified associations (e.g. "Module", or "Discussion.Module" for a nested one) of a record
        /// already in hand, so that reading them is not a synchronous database call.
        /// An association whose ID is empty, or which is not a cached reference, is skipped.
        /// </summary>
        public static Task IncludeAssociations(this IDatabase @this, IEntity entity, params string[] associations)
        {
            if (entity == null) return Task.CompletedTask;
            return @this.IncludeAssociations(new[] { entity }, associations);
        }

        /// <summary>
        /// Loads the specified associations (e.g. "Module", or "Discussion.Module" for a nested one) of records
        /// already in hand, so that reading them is not a synchronous database call.
        /// A record shared by several of them is loaded once. Null records, and associations whose ID is empty,
        /// are skipped.
        /// </summary>
        public static async Task IncludeAssociations(this IDatabase @this, IEnumerable<IEntity> entities,
            params string[] associations)
        {
            var records = entities?.Except(x => x == null).ToArray() ?? new IEntity[0];
            if (records.None()) return;

            var paths = associations.Select(x => x.Split('.')).ToArray();

            // Checked for every record before any is loaded, so that an invalid one does not leave some bound.
            // A nested one can only be checked once the records it is read from are loaded.
            foreach (var type in records.Select(x => x.GetType()).Distinct())
                foreach (var path in paths)
                    GetAssociation(type, path[0]);

            foreach (var path in paths)
                await @this.IncludeAssociation(records, path);
        }

        static async Task IncludeAssociation(this IDatabase @this, IEntity[] entities, string[] path)
        {
            var loaded = new List<IEntity>();

            var ofTypes = entities.GroupBy(x => x.GetType())
                .Select(x => new { Records = x, Members = GetAssociation(x.Key, path[0]) })
                .ToArray(); // So that every type is checked before any is loaded.

            foreach (var ofType in ofTypes)
            {
                var (association, cachedField, idProperty) = ofType.Members;

                var byId = ofType.Records.GroupBy(x => idProperty.GetValue(x))
                    .Except(x => x.Key.ToStringOrEmpty().IsEmpty()) // Reading it returns null without loading anything.
                    .ToArray();

                var found = await @this.LoadByIds(byId.Select(x => x.Key).ToArray(), association.PropertyType);

                foreach (var group in byId)
                {
                    var id = group.Key;

                    // Not found by its exact ID (e.g. "gb" for the record "GB", or a soft deleted record), so it is
                    // loaded by its ID, as before.
                    var associated = found.GetOrDefault(id) ?? await @this.LoadById(id, association.PropertyType);
                    if (associated == null) continue;

                    // Bound to the ID it is read by, rather than its own, which may differ in case for a string ID.
                    foreach (var entity in group)
                    {
                        var cachedRef = cachedField.GetValue(entity);
                        cachedRef.GetType().GetMethod("BindTo", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(cachedRef, new[] { id, associated });
                    }

                    if (loaded.None(x => ReferenceEquals(x, associated))) loaded.Add(associated);
                }
            }

            if (path.Length > 1 && loaded.Any())
                await @this.IncludeAssociation(loaded.ToArray(), path.Skip(1).ToArray());
        }

        static async Task<IEntity> LoadById(this IDatabase @this, object id, Type type) =>
            @this is Database database ?
                await database.FindById(id, type) :
                await @this.GetOrDefault(id, type);

        /// <summary>
        /// Gets the records of the specified IDs that are cached, and loads the others in one query per batch.
        /// Only a record whose ID is exactly one of them is returned, keyed by that ID. The others are left to be
        /// loaded by their IDs, as are all of them when they can't be loaded together.
        /// </summary>
        static async Task<Dictionary<object, IEntity>> LoadByIds(this IDatabase @this, object[] ids, Type type)
        {
            var result = new Dictionary<object, IEntity>();

            // An interface is loaded from each type that implements it, and a [CacheAllRecords] type from its cache.
            if (!(@this is Database database) || ids.Length < 2 || type.IsAbstract ||
                Database.NeedsTypeResolution(type) || database.CanLoadAllRecords(type) ||
                !CanLoadIdList(database.GetProvider(type)) || IdListCriterion(ids) == null)
                return result;

            var toLoad = new List<object>();

            foreach (var id in ids)
            {
                var cached = database.Cache.Get(type, id.ToString());
                if (cached != null) result[id] = cached;
                else toLoad.Add(id);
            }

            if (toLoad.Count < 2) return result;

            foreach (var batch in toLoad.Chop(IdListBatchSize).Select(x => x.ToArray()))
            {
                var query = (DatabaseQuery)database.Of(type).Where(IdListCriterion(batch));

                var loaded = (await query.GetListWithoutQueryCache())
                    .GroupBy(x => x.GetId()).ToDictionary(x => x.Key, x => x.First());

                foreach (var id in batch)
                    if (loaded.TryGetValue(id, out var record)) result[id] = record;
            }

            return result;
        }

        static Criterion IdListCriterion(object[] ids)
        {
            if (ids.All(x => x is Guid)) return new Criterion("ID", FilterFunction.In, ids.Cast<Guid>());
            if (ids.All(x => x is string)) return new Criterion("ID", FilterFunction.In, ids.Cast<string>());
            if (ids.All(x => x is int)) return new Criterion("ID", FilterFunction.In, ids.Cast<int>());
            return null;
        }

        static (PropertyInfo Association, FieldInfo CachedField, PropertyInfo IdProperty) GetAssociation(Type type,
            string name)
        {
            var association = type.GetProperty(name) ??
                throw new ArgumentException($"{type.Name} has no property named {name}.");

            // The same members that Include() binds through.
            var cachedField = association.DeclaringType.GetField("cached" + association.Name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            var idProperty = association.DeclaringType.GetProperty(association.Name + "Id");

            // Skipping it would leave reading it to load it synchronously, which is what this is meant to prevent.
            if (cachedField == null || idProperty == null)
                throw new ArgumentException($"{association.DeclaringType.Name}.{association.Name} is not an " +
                    $"association backed by a cached{association.Name} field and a {association.Name}Id property.");

            return (association, cachedField, idProperty);
        }
    }
}
