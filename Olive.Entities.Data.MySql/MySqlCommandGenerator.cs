using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Olive.Entities.Data
{
    public class MySqlCommandGenerator : SqlCommandGenerator
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
            return query.TakeTop.ToStringOrEmpty()
                .WithPrefix($" LIMIT {query.PageStartIndex}, ");
        }

        protected override string GetInsertCommandTemplate(IDataProviderMetaData metaData)
        {
            var autoNumber = metaData.AutoNumberProperty;

            return $@"INSERT INTO {{0}} ({{1}})
                VALUES ({{2}});
                {"SELECT CAST(LAST_INSERT_ID() as SIGNED);".OnlyWhen(autoNumber != null)}";
        }

        public override string SafeId(string id) => $"`{id}`";

        public override string UnescapeId(string id) => id.Trim('`');
    }
}
