using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// It provides a tree for the association properties
    /// </summary>
    public class AssociationInclusion
    {
        public PropertyInfo Association { get; set; }
        List<string> IncludedNestedAssociations = new List<string>();

        public static AssociationInclusion Create(PropertyInfo association) =>
            new AssociationInclusion { Association = association };

        public void IncludeNestedAssociation(string nestedAssociation)
            => IncludedNestedAssociations.Add(nestedAssociation);

        public async Task LoadAssociations(DatabaseQuery query, IEnumerable<IEntity> mainObjects)
        {
            var cachedField = query.EntityType.GetField("cached" + Association.Name,
                BindingFlags.NonPublic | BindingFlags.Instance);

            var database = Context.Current.Database() as Database;

            // All records of a [CacheAllRecords] type are served from the cache, rather than queried for these objects.
            var fromAllRecords = cachedField != null && IncludedNestedAssociations.None() &&
                database?.CanLoadAllRecords(Association.PropertyType) == true;

            var associatedObjects = fromAllRecords ?
                database.LoadAllRecords(Association.PropertyType) : LoadTheAssociatedObjects(query);

            if (cachedField != null)
                BindAssociations(cachedField, await associatedObjects, mainObjects,
                    ignoreUnreferenced: query.PageSize.HasValue || fromAllRecords);
        }


        internal void BindAssociations(FieldInfo cachedField, IEnumerable<IEntity> associatedObjects,
            IEnumerable<IEntity> mainObjects, bool ignoreUnreferenced)
        {
            var groupedObjects = GroupTheMainObjects(mainObjects);

            foreach (var associatedObject in associatedObjects)
            {
                var group = groupedObjects.GetOrDefault(associatedObject.GetId());
                if (group == null)
                {
                    if (ignoreUnreferenced) continue;

                    // String IDs are matched exactly, but the database may have matched a variant of the key
                    // under its collation (e.g. 'gb' = 'GB', or trailing spaces). Guessing the owner could bind
                    // the wrong record, so leave it unbound: the cached reference will load it on demand.
                    if (associatedObject.GetId() is string) continue;

                    throw new Exception($@"Database include binding failed.
The loaded associated {associatedObject.GetType().Name} with the id {associatedObject.GetId()},
is not referenced by any {Association.DeclaringType.Name} object!
Hint: All associated {Association.Name} Ids are:
{groupedObjects.Select(x => x.Key).ToLinesString()}");
                }

                foreach (var mainEntity in group)
                    BindToCachedField(cachedField, associatedObject, mainEntity);
            }
        }

        void BindToCachedField(FieldInfo cachedField, IEntity associatedObject, IEntity mainEntity)
        {
            var cachedRef = cachedField.GetValue(mainEntity);

            var bindMethod = cachedRef.GetType().GetMethod("Bind",
                BindingFlags.NonPublic | BindingFlags.Instance);

            bindMethod?.Invoke(cachedRef, new[] { associatedObject });
        }

        Dictionary<object, IEntity[]> GroupTheMainObjects(IEnumerable<IEntity> mainObjects)
        {
            var idProperty = Association.DeclaringType.GetProperty(Association.Name + "Id");

            return mainObjects
                .GroupBy(item => idProperty.GetValue(item))
                .Except(group => group.Key == null)
                .ToDictionary(i => i.Key, i => i.ToArray());
        }

        Task<IEntity[]> LoadTheAssociatedObjects(DatabaseQuery query)
        {
            var nestedQuery = Context.Current.Database().Of(Association.PropertyType);
            var provider = ((DatabaseQuery)nestedQuery).Provider;

            return nestedQuery
                       .Where(provider.GetAssociationInclusionCriteria(query, Association))
                       .Include(IncludedNestedAssociations)
                       .GetList();
        }
    }
}