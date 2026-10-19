using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Olive.Entities.Data
{
    public class SqlServerSqlCommandGenerator : SqlCommandGenerator, IBulkCopyCommandGenerator
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

        // ================================================================ direct operations

        /// <summary>The rows a direct command acts on, as a CTE: TOP and ORDER BY work there, and a DELETE or
        /// UPDATE of a CTE over one table writes that table.</summary>
        string Batch(DatabaseQuery query, DirectCommandTarget target, string hints)
        {
            var top = query.TakeTop.HasValue ? $"TOP ({query.TakeTop}) " : "";
            var sort = GenerateSort(query).WithPrefix(" ORDER BY ").OnlyWhen(query.TakeTop.HasValue);

            return $"WITH batch AS (SELECT {top}{target.Alias}.* FROM {target.Tables}{hints.WithWrappers("WITH (", ") ")}" +
                $"{GenerateWhere(query)}{sort})";
        }

        /// <summary>READPAST passes over rows another transaction holds, as far as SQL Server can see them: it
        /// skips a lock it meets on the index it reads, but not one it meets looking a row up in a heap, which
        /// it waits for. So on a heap (a table with no clustered index) a row locked through another index can
        /// still make the statement wait. It also needs READ COMMITTED or REPEATABLE READ.</summary>
        static string SkipLocked(DatabaseQuery query) => "READPAST".OnlyWhen(query.SkipLockedRows);

        public override string GenerateDirectDeleteCommand(DatabaseQuery query, DirectCommandTarget target) =>
            $"{Batch(query, target, SkipLocked(query))} DELETE FROM batch;";

        public override string GenerateDirectUpdateCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            $"{Batch(query, target, SkipLocked(query))} UPDATE batch SET {set};";

        /// <summary>UPDLOCK makes the read take a real lock: under READ_COMMITTED_SNAPSHOT a plain read takes
        /// none, and two claimers would select the same rows.</summary>
        public override string GenerateDirectClaimCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            $"{Batch(query, target, new[] { SkipLocked(query), "UPDLOCK", "ROWLOCK" }.Trim().ToString(", "))} " +
            $"UPDATE batch SET {set} OUTPUT inserted.{target.IdColumn};";

        /// <summary>SqlBulkCopy matches destination columns by exact name, where SQL Server itself ignores case
        /// ("ID" is the key M# creates as Id), so each column is mapped to the table's own spelling of it.
        /// CheckConstraints and FireTriggers make it behave like an INSERT, as PostgreSQL's COPY does: without
        /// them SQL Server skips foreign keys and CHECKs and then marks them untrusted.</summary>
        public async Task BulkCopy(DbConnection connection, DbTransaction transaction, string table, DataTable rows)
        {
            var destination = new List<string>();
            using (var command = new SqlCommand($"SELECT TOP 0 * FROM {table}", (SqlConnection)connection, (SqlTransaction)transaction))
            using (var reader = await command.ExecuteReaderAsync())
                for (var i = 0; i < reader.FieldCount; i++) destination.Add(reader.GetName(i));

            using (var copy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers, (SqlTransaction)transaction)
            { DestinationTableName = table, BulkCopyTimeout = 0 })
            {
                foreach (DataColumn column in rows.Columns)
                    copy.ColumnMappings.Add(column.ColumnName,
                        destination.FirstOrDefault(x => x.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)) ?? column.ColumnName);

                await copy.WriteToServerAsync(rows);
            }
        }

        public override string SafeId(string id) => "[" + id?.Replace("]", "]]") + "]";

        public override string UnescapeId(string id) => id.Trim('[', ']');
    }
}
