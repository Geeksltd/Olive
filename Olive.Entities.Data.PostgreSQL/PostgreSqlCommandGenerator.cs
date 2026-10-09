using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data
{
    public class PostgreSqlCommandGenerator : SqlCommandGenerator
    {
        public override string GenerateSelectCommand(IDatabaseQuery iquery, string tables, string fields)
        {
            var query = (DatabaseQuery)iquery;

            var r = new StringBuilder("SELECT");

            r.AppendLine($" {fields} FROM {tables}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));
            r.AppendLine(GeneratePagination(query));

            return r.ToString();
        }

        public override string GeneratePagination(IDatabaseQuery query)
        {
            var result = (query.PageSize ?? query.TakeTop).ToStringOrEmpty().WithPrefix(" LIMIT ");

            result += query.PageStartIndex.ToString()
                .OnlyWhen(query.PageStartIndex > 0)
                .WithPrefix(" OFFSET ");

            return result;
        }

        public override string GetFullTableName(IDataProviderMetaData metaData) =>
            SafeId(metaData.Schema).OnlyWhen(metaData.Schema.HasValue()).WithSuffix(".") + SafeId(metaData.TableName);

        // Column references use the alias unquoted, which PostgreSQL folds to lower case, so the alias
        // is declared unquoted too. A quoted alias would keep its case and no longer match them.
        public override string SafeAlias(string alias) => NormalizeAliasPart(alias);

        // Nested association aliases are dotted ("Parent.Child_Table"), which an unquoted alias cannot be.
        public override string NormalizeAliasPart(string alias) => alias?.Replace(".", "__");

        protected override string GetInsertCommandTemplate(IDataProviderMetaData metaData)
        {
            var autoNumber = metaData.AutoNumberProperty;

            return "INSERT INTO {0} ({1}) VALUES ({2})" +
                $" RETURNING {SafeId(autoNumber?.Name)}".OnlyWhen(autoNumber != null);
        }

        public override string SafeId(string id) => $"\"{id}\"";

        public override string UnescapeId(string id) => id.Trim('\"');
    }
}
