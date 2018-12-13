using System;
using System.Linq;

namespace Olive.Entities.ObjectDataProvider
{
    class EntityType
    {        
        public string Name, ClassName, ClassFullName, IdColumnName, TableName, PrimaryKeyType, PluralName, AssemblyFullName;

        public Type Type;

        public EntityType BaseType;

        public bool SoftDelete, IsTransient, IsAbstract, HasDataAccessClass,
            HasCustomPrimaryKeyColumn, AssignIDByDatabase;//, HasKnownDatabase;

        public bool HasKnownDatabase => !IsTransient;

        public EntityType[] Derivatives, WithAllParents, AllParents, AllChildren;

        public Property[] AllProperties, Properties, DirectDatabaseFields;

        internal int GetSoftDeletePosition()
        {
            if (!SoftDelete) return 0;

            Property[] fields;
            fields = WithAllParents.SelectMany(x => x.DirectDatabaseFields).ToArray();

            var fieldsString = fields.Select(x => x.Name).ToList();
            return fieldsString.ToList().FindIndex(x => x == $"{TableName}__SoftDeleted");
        }
    }
}