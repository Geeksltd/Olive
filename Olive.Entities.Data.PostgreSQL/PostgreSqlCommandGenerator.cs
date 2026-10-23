using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace Olive.Entities.Data
{
    public class PostgreSqlCommandGenerator : SqlCommandGenerator, IBulkCopyCommandGenerator
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

        // ================================================================ report queries

        public override string GenerateReportCommand(ReportCommand command)
        {
            var r = new StringBuilder("SELECT ");
            if (command.Distinct) r.Append("DISTINCT ");
            r.Append($"{command.Columns} FROM {command.Tables}{command.Clauses}");

            r.Append((command.PageSize ?? command.Top).ToStringOrEmpty().WithPrefix(" LIMIT "));
            if (command.PageSize.HasValue && command.PageStartIndex > 0) r.Append($" OFFSET {command.PageStartIndex}");

            return r.ToString();
        }

        public override string TruncateToHour(string expression) => $"date_trunc('hour', {expression})";

        public override string TruncateToDay(string expression) => $"date_trunc('day', {expression})";

        public override string TruncateToMinutes(string expression, int minutes) =>
            $"date_bin('{minutes} minutes', {expression}, TIMESTAMP '2000-01-01')";

        /// <summary>DOW counts Sunday as 0, as C# does.</summary>
        public override string DayOfWeek(string expression) => $"EXTRACT(DOW FROM {expression})";

        public override string HourOfDay(string expression) => $"EXTRACT(HOUR FROM {expression})";

        public override string IndexOf(string text, string find) => $"strpos({text}, {find})";

        /// <summary>The collation Olive gives text columns (see PostgreSqlScriptTranslator).</summary>
        public override string CaseInsensitive(string expression) => $"{expression} COLLATE {PostgreSqlScriptTranslator.Collation}";

        public override string ToDouble(string expression) => $"CAST({expression} AS double precision)";

        public override string GuidAsText(string expression) => $"CAST({expression} AS text)";

        public override string GetFullTableName(IDataProviderMetaData metaData) =>
            SafeSchema(metaData.Schema).OnlyWhen(metaData.Schema.HasValue()).WithSuffix(".") + SafeId(metaData.TableName);

        /// <summary>
        /// Schemas are lower case, whatever case an entity's [Schema] attribute uses: that is how M# creates
        /// them on SQL Server, so a database migrated from one keeps its schema names as they are, and it is
        /// PostgreSQL's own spelling of an unquoted name. Tables and columns keep their exact case.
        /// </summary>
        public override string SafeSchema(string schema) => SafeId(schema?.ToLowerInvariant());

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

        // ================================================================ direct operations

        /// <summary>The keys of the rows a direct command acts on: DELETE and UPDATE take no LIMIT or ORDER BY
        /// here. FOR UPDATE … SKIP LOCKED passes over rows another transaction holds.</summary>
        string Keys(DatabaseQuery query, DirectCommandTarget target, bool lockRows) =>
            $"SELECT {target.Alias}.{target.IdColumn} FROM {target.Tables}{GenerateWhere(query)}" +
            GenerateSort(query).WithPrefix(" ORDER BY ") +
            query.TakeTop.ToStringOrEmpty().WithPrefix(" LIMIT ") +
            $" FOR UPDATE OF {target.Alias}".OnlyWhen(lockRows || query.SkipLockedRows) +
            " SKIP LOCKED".OnlyWhen(query.SkipLockedRows);

        /// <summary>Whether every matching row is affected, so the statement can filter its own rows.</summary>
        static bool AllRows(DatabaseQuery query) => !query.TakeTop.HasValue && !query.SkipLockedRows;

        public override string GenerateDirectDeleteCommand(DatabaseQuery query, DirectCommandTarget target) =>
            AllRows(query)
                ? $"DELETE FROM {target.Tables}{GenerateWhere(query)};"
                : $"DELETE FROM {target.Table} WHERE {target.IdColumn} IN ({Keys(query, target, lockRows: false)});";

        public override string GenerateDirectUpdateCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            AllRows(query)
                ? $"UPDATE {target.Tables}SET {set}{GenerateWhere(query)};"
                : $"UPDATE {target.Table} SET {set} WHERE {target.IdColumn} IN ({Keys(query, target, lockRows: false)});";

        public override string GenerateDirectClaimCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            $"WITH batch AS ({Keys(query, target, lockRows: true)}) " +
            $"UPDATE {target.Table} SET {set} FROM batch WHERE {target.Table}.{target.IdColumn} = batch.{target.IdColumn} " +
            $"RETURNING {target.Table}.{target.IdColumn};";

        /// <summary>A binary COPY. Binary COPY takes each value in exactly its column's type, so the columns'
        /// types are read first and every value converted to its column's (an entity's double into a numeric
        /// column, say). Times go in as they are, with no zone, as <see cref="PostgreSqlParameterFactory"/>
        /// writes them.</summary>
        /// <remarks>An open transaction needs nothing passed: Npgsql runs the COPY in the connection's.</remarks>
        public async Task BulkCopy(DbConnection dbConnection, DbTransaction transaction, string table, DataTable rows)
        {
            var columns = rows.Columns.Cast<DataColumn>().ToArray();
            var names = columns.Select(c => SafeId(c.ColumnName)).ToString(", ");
            var connection = (NpgsqlConnection)dbConnection;

            string[] types;
            using (var command = new NpgsqlCommand($"SELECT {names} FROM {table} WHERE 1 = 0", connection))
            using (var reader = await command.ExecuteReaderAsync())
                // Without the size or precision a name can carry ("numeric(18,4)", "character varying(200)").
                types = Enumerable.Range(0, columns.Length)
                    .Select(i => System.Text.RegularExpressions.Regex.Replace(reader.GetDataTypeName(i), @"\(.*?\)", "").Trim())
                    .ToArray();

            using (var writer = await connection.BeginBinaryImportAsync($"COPY {table} ({names}) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (DataRow row in rows.Rows)
                {
                    await writer.StartRowAsync();
                    for (var i = 0; i < columns.Length; i++)
                        await Write(writer, row[columns[i]], types[i], table, columns[i].ColumnName);
                }

                await writer.CompleteAsync();
            }
        }

        static Task Write(NpgsqlBinaryImporter writer, object value, string type, string table, string column)
        {
            if (value == null || value is DBNull) return writer.WriteNullAsync();
            if (value is Enum) value = Convert.ToInt32(value);

            switch (type)
            {
                case "uuid": return writer.WriteAsync(value is Guid guid ? guid : Guid.Parse(value.ToString()), NpgsqlDbType.Uuid);
                case "text":
                case "character varying":
                case "character": return writer.WriteAsync(value.ToString(), NpgsqlDbType.Text);
                case "boolean": return writer.WriteAsync(Convert.ToBoolean(value), NpgsqlDbType.Boolean);
                case "smallint": return writer.WriteAsync(Convert.ToInt16(value), NpgsqlDbType.Smallint);
                case "integer": return writer.WriteAsync(Convert.ToInt32(value), NpgsqlDbType.Integer);
                case "bigint": return writer.WriteAsync(Convert.ToInt64(value), NpgsqlDbType.Bigint);
                case "numeric": return writer.WriteAsync(Convert.ToDecimal(value), NpgsqlDbType.Numeric);
                case "real": return writer.WriteAsync(Convert.ToSingle(value), NpgsqlDbType.Real);
                case "double precision": return writer.WriteAsync(Convert.ToDouble(value), NpgsqlDbType.Double);
                case "bytea": return writer.WriteAsync((byte[])value, NpgsqlDbType.Bytea);
                case "timestamp without time zone":
                    return writer.WriteAsync(DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Unspecified), NpgsqlDbType.Timestamp);
                case "timestamp with time zone":
                    return writer.WriteAsync(DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc), NpgsqlDbType.TimestampTz);
                case "date":
                    return writer.WriteAsync(value is DateTime date ? DateOnly.FromDateTime(date) : (DateOnly)value, NpgsqlDbType.Date);
                case "interval": return writer.WriteAsync((TimeSpan)value, NpgsqlDbType.Interval);
                default: throw new NotSupportedException($"BulkCopy cannot write to {table}.{column}, a {type} column.");
            }
        }

        public override string SafeId(string id) => $"\"{id}\"";

        public override string UnescapeId(string id) => id.Trim('\"');

        /// <summary>Npgsql reports a command timeout as a TimeoutException inside its own, or (when it managed to
        /// cancel the query on the server) as the server's query_canceled, 57014.</summary>
        public override bool IsTimeout(Exception exception) =>
            exception is NpgsqlException npgsql && npgsql.InnerException is TimeoutException ||
            exception is PostgresException postgres && postgres.SqlState == PostgresErrorCodes.QueryCanceled ||
            base.IsTimeout(exception);
    }
}
