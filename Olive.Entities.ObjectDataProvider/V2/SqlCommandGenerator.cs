using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Olive.Entities.Data;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public abstract class SqlCommandGenerator
    {

        protected abstract string SafeId(string objectName);

        protected abstract string UnescapeId(string id);

        public abstract string GenerateSelectCommand(IDatabaseQuery iquery, string tables, string fields);

        protected virtual string GenerateSort(DatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => query.Column(p.Property) + " DESC".OnlyWhen(p.Descending)));

            var offset = string.Empty;
            if (query.PageSize > 0)
                offset = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            return parts.ToString(", ") + offset;
        }

        public virtual string GenerateWhere(DatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("ID")} IS NOT NULL");

            foreach (var c in query.Criteria)
                r.Append(Generate(query, c).WithPrefix(" AND "));

            return r.ToString();
        }

        public virtual string GenerateUpdateCommand(DataProviderMetaData metaData)
        {
            var properties = metaData.UserDefienedProperties
                .Except(p => p.IsAutoNumber);

            return $@"UPDATE {GetFullTablaName(metaData)} SET
                {properties.Select(x => $"{SafeId(x.Name)} = @{x.ParameterName}").ToString(", ")}
                OUTPUT INSERTED.{metaData.IdColumnName}
                WHERE {metaData.IdColumnName} = @OriginalId";
        }

        public virtual string GenerateInsertCommand(DataProviderMetaData metaData)
        {
            var properties = metaData.UserDefienedAndIdProperties
                .Except(p => p.IsAutoNumber);

            if (metaData.IsSoftDeleteEnabled)
                properties = properties.Concat(metaData.Properties.First(p => p.IsDeleted));

            var autoNumber = metaData.Properties.FirstOrDefault(x => x.IsAutoNumber);

            return $@"INSERT INTO {GetFullTablaName(metaData)}
                ({properties.Select(x => SafeId(x.Name)).ToString(", ")})
                {$"OUTPUT [INSERTED].{autoNumber?.Name}".OnlyWhen(autoNumber != null)}
                VALUES
                ({properties.Select(x => $"@{x.ParameterName}").ToString(", ")})";
        }

        public virtual string GenerateDeleteCommand(DataProviderMetaData metaData) =>
            $"DELETE FROM {GetFullTablaName(metaData)} WHERE {metaData.IdColumnName} = @Id";

        string GetFullTablaName(DataProviderMetaData metaData) =>
            metaData.Schema.WithSuffix(".") + metaData.TableName;

        string Generate(DatabaseQuery query, ICriterion criterion)
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
