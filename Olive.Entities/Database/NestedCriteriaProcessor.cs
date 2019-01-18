using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities
{
    public class NestedCriteriaProcessor
    {
        public List<AssociationSubQuery> Queries = new List<AssociationSubQuery>();
        public PropertyInfo Property;
        public string TableAlias;

        public NestedCriteriaProcessor(Type declaringType, string[] parts)
        {
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                Property = declaringType.GetProperty(part);
                if (i == parts.Length - 1)
                {
                    // Last one is the actual end property.
                    FixId();
                    break;
                }
                else
                {
                    var provider = Context.Current.Database().GetProvider(declaringType);

                    var mappedSubquery = provider.MapSubquery(part + ".*", TableAlias);

                    Queries.Add(new AssociationSubQuery(Property, mappedSubquery));

                    declaringType = Property.PropertyType;

                    var propertyTableType = TableNameAttribute.GetTableName(declaringType.GetProperty(parts[i + 1]).DeclaringType);

                    TableAlias += "." + part + "_" + propertyTableType;
                }
            }
        }

        void FixId()
        {
            if (!Property.Name.EndsWith("Id")) return;
            if (Property.PropertyType != typeof(Guid) && Property.PropertyType != typeof(Guid?)) return;

            var entityProperty = Property.DeclaringType.GetProperty(Property.Name.TrimEnd(2));
            if (entityProperty != null &&
                entityProperty.PropertyType.IsA<IEntity>() &&
                !entityProperty.Defines<CalculatedAttribute>())
                Property = entityProperty;
        }
    }
}