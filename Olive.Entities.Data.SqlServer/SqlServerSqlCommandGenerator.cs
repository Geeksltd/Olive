using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data
{
    public class SqlServerSqlCommandGenerator : SqlCommandGenerator
    {

        public override string GenerateSelectCommand(IDatabaseQuery iquery, string tables, string fields)
        {
            var query = (DatabaseQuery)iquery;

            var r = new StringBuilder("SELECT");

            r.Append(query.TakeTop.ToStringOrEmpty().WithPrefix(" TOP "));
            r.AppendLine($" {fields} FROM {tables}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));
            r.AppendLine(GeneratePagination(query));

            return r.ToString();
        }

        public override string GenerateSort(IDatabaseQuery query)
        {
            if (query.PageSize.HasValue && query.OrderByParts.None()) return "(SELECT NULL)";

            return base.GenerateSort(query);
        }

        public override string GeneratePagination(IDatabaseQuery query)
        {
            var result = string.Empty;

            if (query.PageSize > 0)
                result = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            return result;
        }

        public override string SafeId(string id) => "[" + id + "]";

        public override string UnescapeId(string id) => id.Trim('[', ']');
    }
}
