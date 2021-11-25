using System;
using System.Linq;
using System.Reflection;

namespace Olive.Entities
{
    public class AssociationSubQuery
    {
        public PropertyInfo Property { get; set; }
        public string ColumnName { get; set; }
        public string MappedSubquery { get; set; }
        public string TableAlias { get; set; }
        public string RecordIdExpression { get; set; }
        public string SelectClause { get; set; }

        public AssociationSubQuery(PropertyInfo property, string mappedSubquery)
        {
            Property = property;

            var subquerySplitted = mappedSubquery.Split(new[] { " WHERE " }, StringSplitOptions.RemoveEmptyEntries)
                .Trim().ToArray();

            SelectClause = subquerySplitted[0].ToLines().Trim().ToString(" ");

            var whereClauseSplitted = subquerySplitted[1].Split('=').Trim().ToArray();
            ColumnName = whereClauseSplitted[0];
            TableAlias = whereClauseSplitted.Last().Split('.').ExceptLast().ToString(".").Trim();

            RecordIdExpression = whereClauseSplitted[1];
        }

        public override string ToString() => SelectClause + " WHERE " + ColumnName + " = " + RecordIdExpression;
    }
}