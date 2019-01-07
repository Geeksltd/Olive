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

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.AppendLine($" {fields} FROM {tables}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));
            r.AppendLine(query.TakeTop.ToStringOrEmpty().WithPrefix(" LIMIT "));

            return r.ToString();
        }

        public override string SafeId(string id) => $"\"{id}\"";

        public override string UnescapeId(string id) => id.Trim('\"');
    }
}
