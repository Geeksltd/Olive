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

        /// <summary>READPAST with READCOMMITTEDLOCK, which reads the table at READ COMMITTED whatever the
        /// session's level: a pooled connection keeps the level of the last transaction it ran, and at
        /// SERIALIZABLE or SNAPSHOT a bare READPAST fails ("You can only specify the READPAST lock in the READ
        /// COMMITTED or REPEATABLE READ isolation levels").</summary>
        static string SkipLockedAtAnyLevel(DatabaseQuery query) => "READPAST, READCOMMITTEDLOCK".OnlyWhen(query.SkipLockedRows);

        public override string GenerateDirectDeleteCommand(DatabaseQuery query, DirectCommandTarget target) =>
            $"{Batch(query, target, SkipLockedAtAnyLevel(query))} DELETE FROM batch;";

        public override string GenerateDirectUpdateCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            $"{Batch(query, target, SkipLockedAtAnyLevel(query))} UPDATE batch SET {set};";

        /// <summary>UPDLOCK makes the read take a real lock: under READ_COMMITTED_SNAPSHOT a plain read takes
        /// none, and two claimers would select the same rows. Not READCOMMITTEDLOCK: with UPDLOCK, SQL Server
        /// still refuses READPAST at SERIALIZABLE or SNAPSHOT, so a claim needs the session at READ COMMITTED or
        /// REPEATABLE READ.</summary>
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

        // ================================================================ report queries

        /// <summary>A sorted query's rows are limited with OFFSET … FETCH (Top as the first page), an unsorted
        /// one's with TOP, since OFFSET needs an ORDER BY.</summary>
        public override string GenerateReportCommand(ReportCommand command)
        {
            var sorted = command.OrderBy.HasValue();
            var start = command.PageSize.HasValue ? command.PageStartIndex : 0;
            var take = command.PageSize ?? command.Top;

            var r = new StringBuilder("SELECT ");
            if (command.Distinct) r.Append("DISTINCT ");
            if (take.HasValue && !sorted && !command.PageSize.HasValue) r.Append($"TOP ({take}) ");
            r.Append($"{command.Columns} FROM {command.Tables}{command.Clauses}");

            if (take.HasValue && (sorted || command.PageSize.HasValue))
            {
                if (!sorted) r.Append(" ORDER BY (SELECT NULL)");
                r.Append($" OFFSET {start} ROWS FETCH NEXT {take} ROWS ONLY");
            }

            return r.ToString();
        }

        public override string TruncateToHour(string expression) => $"DATEADD(hour, DATEDIFF(hour, 0, {expression}), 0)";

        public override string TruncateToDay(string expression) => $"DATEADD(day, DATEDIFF(day, 0, {expression}), 0)";

        public override string TruncateToMinutes(string expression, int minutes) =>
            $"DATEADD(minute, DATEDIFF(minute, 0, {expression}) / {minutes} * {minutes}, 0)";

        /// <summary>Through @@DATEFIRST, so Sunday is 0 whatever the session's first day of the week.</summary>
        public override string DayOfWeek(string expression) => $"((DATEPART(weekday, {expression}) + @@DATEFIRST - 1) % 7)";

        public override string HourOfDay(string expression) => $"DATEPART(hour, {expression})";

        public override string IndexOf(string text, string find) => $"CHARINDEX({find}, {text})";

        public override string CaseInsensitive(string expression) => $"{expression} COLLATE Latin1_General_CI_AS";

        public override string ToDouble(string expression) => $"CAST({expression} AS float)";

        public override string GuidAsText(string expression) => $"CAST({expression} AS nvarchar(36))";

        /// <summary>N'', so a literal keeps characters outside the database's code page.</summary>
        public override string StringLiteral(string value) => "N" + base.StringLiteral(value);

        public override string SafeId(string id) => "[" + id?.Replace("]", "]]") + "]";

        public override string UnescapeId(string id) => id.Trim('[', ']');
    }
}
