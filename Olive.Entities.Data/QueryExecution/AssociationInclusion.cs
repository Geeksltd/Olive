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

            if (cachedField == null) return;

            var groupedObjects = GroupTheMainObjects(mainObjects);

            foreach (var associatedObject in await LoadTheAssociatedObjects(query, groupedObjects.Keys))
            {
                var group = groupedObjects.GetOrDefault(associatedObject.GetId());
                if (group == null)
                {
                    if (query.PageSize.HasValue) continue;

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

        Task<IEntity[]> LoadTheAssociatedObjects(DatabaseQuery query, ICollection<object> associatedIds)
        {
            var nestedQuery = Context.Current.Database().Of(Association.PropertyType);
            var provider = ((DatabaseQuery)nestedQuery).Provider;

            ICriterion criterion = null;

            if (query.TakeTop.HasValue || query.PageSize.HasValue)
            {
                // The main query returned only a window of its matching rows (Top(), FirstOrDefault() or paging).
                // Running it again as a sub-query can return a different window, because without a fully
                // deterministic sort the database is free to pick any rows for TOP / OFFSET.
                // That would load the associated objects of the wrong rows, so filter on the loaded ids instead.
                if (associatedIds.None()) return Task.FromResult(new IEntity[0]);

                criterion = CreateIdsCriterion(associatedIds);
            }

            criterion ??= provider.GetAssociationInclusionCriteria(query, Association);

            return nestedQuery
                       .Where(criterion)
                       .Include(IncludedNestedAssociations)
                       .GetList();
        }

        /// <summary>
        /// Creates an "ID IN (...)" criterion for the specified ids.
        /// It returns null for an unsupported ID type, in which case the caller should fall back to a sub-query.
        /// </summary>
        static ICriterion CreateIdsCriterion(IEnumerable<object> ids)
        {
            var items = ids.ExceptNull().ToArray();

            if (items.All(x => x is Guid)) return new Criterion("ID", FilterFunction.In, items.Cast<Guid>());
            if (items.All(x => x is int)) return new Criterion("ID", FilterFunction.In, items.Cast<int>());
            if (items.All(x => x is string)) return new Criterion("ID", FilterFunction.In, items.Cast<string>());

            return null;
        }
    }
}