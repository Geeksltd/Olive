namespace Olive.Entities.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using MySql.Data.MySqlClient;

    public abstract class MySqlDataProvider<TTargetEntity> : MySqlDataProvider where TTargetEntity : IEntity
    {
        public override Type EntityType => typeof(TTargetEntity);
    }

    public abstract partial class MySqlDataProvider : DataProvider<MySqlConnection, MySqlParameter>
    {
        public override IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            var value = data.Value;

            var result = new MySqlParameter { Value = value, ParameterName = data.Key.Remove(" ") };

            if (value is DateTime)
            {
                result.DbType = DbType.DateTime2;
                result.MySqlDbType = MySqlDbType.DateTime;
            }

            return result;
        }

        public override string GenerateSelectCommand(IDatabaseQuery iquery, string fields)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.AppendLine($" {fields} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));
            r.AppendLine(query.TakeTop.ToStringOrEmpty().WithPrefix(" LIMIT "));

            return r.ToString();
        }

        public override string GenerateWhere(DatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("ID")} IS NOT NULL");

            var whereGenerator = new MySqlCriterionGenerator(query);
            foreach (var c in query.Criteria)
                r.Append(whereGenerator.Generate(c).WithPrefix(" AND "));

            return r.ToString();
        }

        protected override string GenerateAssociationLoadingCriteria(string id, string uniqueItems, PropertyInfo association)
        {
            return $"{id} IN (SELECT `{association.Name}` FROM ({uniqueItems}) Alias)";
        }

        public override string GenerateSort(DatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => MapColumn(p.Property) + " DESC".OnlyWhen(p.Descending)));

            var offset = string.Empty;
            if (query.PageSize > 0)
                offset = $" LIMIT {query.PageStartIndex},{query.PageSize}";

            return parts.ToString(", ") + offset;
        }
    }
}