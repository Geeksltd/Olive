namespace Olive.Entities.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    partial class DatabaseQuery
    {
        public string MapColumn(string propertyName) => AliasPrefix + Provider.MapColumn(propertyName);

        public string AliasPrefix { get; set; }

        IDatabaseQuery IDatabaseQuery.WhereIn(string myField, IDatabaseQuery subquery, string targetField)
        {
            return WhereSubquery(myField, subquery, targetField, "IN");
        }

        IDatabaseQuery IDatabaseQuery.WhereNotIn(string myField, IDatabaseQuery subquery, string targetField)
        {
            return WhereSubquery(myField, subquery, targetField, "NOT IN");
        }

        IDatabaseQuery WhereSubquery(string myField, IDatabaseQuery subquery, string targetField, string @operator)
        {
            subquery.AliasPrefix = "Subq" + Guid.NewGuid().ToString().Remove("-").Substring(0, 6);

            var sql = subquery.Provider
                .GenerateSelectCommand(subquery, subquery.MapColumn(targetField));

            sql = $"{MapColumn(myField)} {@operator} ({sql})";
            Criteria.Add(Criterion.FromSql(sql));

            foreach (var subQueryParam in subquery.Parameters)
                Parameters.Add(subQueryParam.Key, subQueryParam.Value);

            return this;
        }
    }
}