using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Entities.Data
{
    public abstract class SqlCommandGenerator : ISqlCommandGenerator, ISchemaNamingSqlCommandGenerator
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
                .Except(p => p.IsAutoNumber || p.IsComputed);

            if (properties.None()) return "";

            return $@"UPDATE {GetFullTableName(metaData)} SET
                {properties.Select(x => $"{SafeId(x.Name)} = @{x.ParameterName}").ToString(", ")}
                WHERE {SafeId(metaData.IdColumnName)} = @OriginalId";
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
            $"DELETE FROM {GetFullTableName(metaData)} WHERE {SafeId(metaData.IdColumnName)} = @Id";

        /// <summary>
        /// A DELETE of the rows the query matches: at most its Top, taken in its sort order, and passing over
        /// locked rows when <see cref="DatabaseQuery.SkipLockedRows"/> is set and the database can. Not
        /// supported unless a provider overrides it.
        /// </summary>
        public virtual string GenerateDirectDeleteCommand(DatabaseQuery query, DirectCommandTarget target) =>
            throw new NotSupportedException($"{GetType().Name} cannot delete bypassing the entity pipeline.");

        /// <summary>An UPDATE of the rows the query matches, as <see cref="GenerateDirectDeleteCommand"/> picks
        /// them. <paramref name="set"/> is the SET list, its columns unqualified.</summary>
        public virtual string GenerateDirectUpdateCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            throw new NotSupportedException($"{GetType().Name} cannot update bypassing the entity pipeline.");

        /// <summary>A statement that takes the rows the query matches (its first Top in its sort order, if set), locking them against
        /// another claim (and passing over those another claim holds, with SkipLockedRows), updates them with
        /// <paramref name="set"/>, and returns their keys as its only column.</summary>
        public virtual string GenerateDirectClaimCommand(DatabaseQuery query, DirectCommandTarget target, string set) =>
            throw new NotSupportedException($"{GetType().Name} does not support Claim().");

        // ================================================================ report queries

        /// <summary>The SELECT of a report query (see ReportQueryExtensions), from its parts. Not supported
        /// unless a provider overrides it, nor are the functions below that have no standard spelling.</summary>
        public virtual string GenerateReportCommand(ReportCommand command) =>
            throw new NotSupportedException($"{GetType().Name} does not support report queries.");

        NotSupportedException NoReportFunction(string name) =>
            new NotSupportedException($"{GetType().Name} does not support {name}() in report queries.");

        /// <summary>The time with its minutes, seconds and fractions removed.</summary>
        public virtual string TruncateToHour(string expression) => throw NoReportFunction(nameof(TruncateToHour));

        /// <summary>The time with its time of day removed, still a date and time.</summary>
        public virtual string TruncateToDay(string expression) => throw NoReportFunction(nameof(TruncateToDay));

        /// <summary>The start of the window of that many minutes the time falls in.</summary>
        public virtual string TruncateToMinutes(string expression, int minutes) => throw NoReportFunction(nameof(TruncateToMinutes));

        /// <summary>The day of the week as C# numbers it: Sunday is 0.</summary>
        public virtual string DayOfWeek(string expression) => throw NoReportFunction(nameof(DayOfWeek));

        /// <summary>The hour of the day, 0 to 23.</summary>
        public virtual string HourOfDay(string expression) => throw NoReportFunction(nameof(HourOfDay));

        /// <summary>The 1-based position of <paramref name="find"/> in <paramref name="text"/>, 0 if absent.</summary>
        public virtual string IndexOf(string text, string find) => throw NoReportFunction(nameof(IndexOf));

        /// <summary>The text under a case-insensitive collation, whatever its column's.</summary>
        public virtual string CaseInsensitive(string expression) => throw NoReportFunction(nameof(CaseInsensitive));

        /// <summary>The value as a double-precision float.</summary>
        public virtual string ToDouble(string expression) => throw NoReportFunction(nameof(ToDouble));

        /// <summary>A key (uniqueidentifier, uuid) as text, which MIN and MAX can take where the key cannot.</summary>
        public virtual string GuidAsText(string expression) => throw NoReportFunction(nameof(GuidAsText));

        /// <summary>The first <paramref name="length"/> characters.</summary>
        public virtual string Left(string expression, int length) => $"LEFT({expression}, {length})";

        /// <summary>The text without leading or trailing spaces.</summary>
        public virtual string Trim(string expression) => $"LTRIM(RTRIM({expression}))";

        /// <summary>The parts joined, a null part read as empty text.</summary>
        public virtual string Concat(IEnumerable<string> parts) => $"CONCAT({parts.ToString(", ")})";

        /// <summary>A text literal, for where SQL needs a value written into the command.</summary>
        public virtual string StringLiteral(string value) => "'" + value.Replace("'", "''") + "'";

        /// <summary>A schema name as this database spells it. The same as SafeId unless a provider overrides it.</summary>
        public virtual string SafeSchema(string schema) => SafeId(schema);

        public virtual string GetFullTableName(IDataProviderMetaData metaData) =>
            metaData.Schema.WithSuffix(".") + metaData.TableName;

        /// <summary>
        /// Escapes a table alias. Column references use the plain alias (so a sub-query prefix can be
        /// prepended to it), which providers with case-sensitive quoted identifiers must match here.
        /// </summary>
        public virtual string SafeAlias(string alias) => SafeId(alias);

        /// <summary>
        /// Rewrites a raw alias fragment before it is substituted into an already escaped template.
        /// </summary>
        public virtual string NormalizeAliasPart(string alias) => alias;

        // ================================================================ transactions and errors

        /// <summary>Marks a point in the current transaction to roll back to (see
        /// DataAccessBehaviourExtensions.BestEffort). Standard SQL, as PostgreSQL, MySQL and SQLite spell it.</summary>
        public virtual string GenerateSavepoint(string name) => $"SAVEPOINT {name}";

        /// <summary>Undoes the transaction's work since the savepoint, and leaves the transaction usable.</summary>
        public virtual string GenerateRollbackToSavepoint(string name) => $"ROLLBACK TO SAVEPOINT {name}";

        /// <summary>Forgets the savepoint once the work after it has succeeded, or null where the database
        /// has no such statement.</summary>
        public virtual string GenerateReleaseSavepoint(string name) => $"RELEASE SAVEPOINT {name}";

        /// <summary>Whether this exception itself, not one it wraps, is this database's command timeout
        /// (see DataAccessBehaviourExtensions.IsTimeout). A plain TimeoutException unless a provider knows better.</summary>
        public virtual bool IsTimeout(Exception exception) => exception is TimeoutException;

        string Generate(IDatabaseQuery query, ICriterion criterion)
        {
            if (criterion == null) return "(1 = 1)";
            else return criterion.ToSql(new SqlConversionContext
            {
                Query = query,
                Type = query.EntityType,
                ToSafeId = SafeId,
                ToSafeAlias = SafeAlias
            });
        }
    }
}
