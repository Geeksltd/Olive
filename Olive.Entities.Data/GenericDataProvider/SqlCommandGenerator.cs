using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Entities.Data
{
    public abstract class SqlCommandGenerator : ISqlCommandGenerator
    {
        public abstract string SafeId(string objectName);

        public abstract string UnescapeId(string id);

        public abstract string GenerateSelectCommand(IDatabaseQuery iquery, string tables, string fields);

        public virtual string GenerateSort(IDatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => query.Column(p.Property) + " DESC".OnlyWhen(p.Descending)));

            return parts.ToString(", ");
        }

        public abstract string GeneratePagination(IDatabaseQuery query);

        public virtual string GenerateWhere(IDatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("ID")} IS NOT NULL");

            foreach (var c in query.Criteria)
                r.Append(Generate(query, c).WithPrefix(" AND "));

            return r.ToString();
        }

        public virtual string GenerateUpdateCommand(IDataProviderMetaData metaData)
        {
            var properties = metaData.UserDefienedAndDeletedProperties
                .Except(p => p.IsAutoNumber);

            if (properties.None()) return "";

            return $@"UPDATE {GetFullTableName(metaData)} SET
                {properties.Select(x => $"{SafeId(x.Name)} = @{x.ParameterName}").ToString(", ")}
                WHERE {metaData.IdColumnName} = @OriginalId";
        }

        public virtual string GenerateInsertCommand(IDataProviderMetaData metaData)
        {
            var properties = metaData.GetPropertiesForInsert();

            return GetInsertCommandTemplate(metaData).FormatWith(
                GetFullTableName(metaData),
                properties.Select(x => SafeId(x.Name)).ToString(", "),
                properties.Select(x => $"@{x.ParameterName}").ToString(", ")
            );
        }

        protected virtual string GetInsertCommandTemplate(IDataProviderMetaData metaData)
        {
            var autoNumber = metaData.AutoNumberProperty;

            return $@"INSERT INTO {{0}} ({{1}})
                {$"OUTPUT [INSERTED].{autoNumber?.Name}".OnlyWhen(autoNumber != null)}
                VALUES ({{2}})";
        }

        public virtual string GenerateDeleteCommand(IDataProviderMetaData metaData) =>
            $"DELETE FROM {GetFullTableName(metaData)} WHERE {metaData.IdColumnName} = @Id";

        protected string GetFullTableName(IDataProviderMetaData metaData) =>
            metaData.Schema.WithSuffix(".") + metaData.TableName;

        string Generate(IDatabaseQuery query, ICriterion criterion)
        {
            if (criterion == null) return "(1 = 1)";
            else return criterion.ToSql(new SqlConversionContext
            {
                Query = query,
                Type = query.EntityType,
                ToSafeId = SafeId
            });
        }
    }
}
