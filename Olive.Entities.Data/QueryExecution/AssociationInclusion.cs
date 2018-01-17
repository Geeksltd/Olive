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
            var associatedObjects = LoadTheAssociatedObjects(query);

            var groupedObjects = GroupTheMainObjects(mainObjects);

            var cachedField = query.EntityType.GetField("cached" + Association.Name,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (cachedField != null)
                foreach (var associatedObject in await associatedObjects)
                    foreach (var mainEntity in groupedObjects[associatedObject.GetId()])
                        BindToCachedField(cachedField, associatedObject, mainEntity);
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

            var groupedResult = mainObjects.GroupBy(item => idProperty.GetValue(item))
           .ToDictionary(i => i.Key, i => i.ToArray());
            return groupedResult;
        }

        Task<IEnumerable<IEntity>> LoadTheAssociatedObjects(DatabaseQuery query)
        {
            return Database.Instance.Of(Association.PropertyType)
                       .Where(query.Provider.GetAssociationInclusionCriteria(query, Association))
                       .Include(IncludedNestedAssociations)
                       .GetList();
        }
    }
}