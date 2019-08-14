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
            var result = query.TakeTop.ToStringOrEmpty().WithPrefix(" LIMIT ");

            result += query.PageStartIndex.ToString()
                .OnlyWhen(query.PageStartIndex > 0)
                .WithPrefix(" OFFSET ");

            return result;
        }

        public override string SafeId(string id) => $"\"{id}\"";

        public override string UnescapeId(string id) => id.Trim('\"');
    }
}
