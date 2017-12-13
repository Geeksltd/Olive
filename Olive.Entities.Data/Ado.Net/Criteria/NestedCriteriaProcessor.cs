using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities.Data
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
                if (i == parts.Length - 1) break;

                var provider = Database.Instance.GetProvider(declaringType);

                var mappedSubquery = provider.MapSubquery(part + ".*", TableAlias);

                Queries.Add(new AssociationSubQuery(Property, mappedSubquery));

                declaringType = Property.PropertyType;
                TableAlias += "." + part;
            }
        }
    }
}