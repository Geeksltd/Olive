namespace Olive.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Olive.Entities.Data;

    /// <summary>
    /// Report queries: groups, totals and projections that the database computes, read back as typed rows
    /// rather than entities. They are plain reads: nothing is cached, saved or raised.
    /// <code>
    /// var rows = await Database.Of&lt;MetricDataPoint&gt;()
    ///     .Where(p => p.Definition.AccountId == account &amp;&amp; p.Timestamp >= from)
    ///     .GroupBy(p => new { p.Definition.ServiceId, Day = p.Timestamp.Date })
    ///     .Select(g => new { g.Key.ServiceId, g.Key.Day,
    ///         Requests = g.Where(p => p.Definition.Key == "api.requests").Sum(p => p.Value),
    ///         Points = g.Count() })
    ///     .OrderByDescending(r => r.Requests).Top(10)
    ///     .GetList();
    /// </code>
    /// <para>
    /// The lambdas are translated, not run. A property path such as <c>p.Definition.Key</c> joins the
    /// associated table; end it with the association's Id property (<c>p.Definition.ServiceId</c>) to read
    /// the key itself. Inside a group: <c>g.Key</c> (or its members), <c>g.Count()</c>,
    /// <c>g.Count(condition)</c>, <c>g.Sum/Min/Max/Average(x => ...)</c>, <c>g.Where(condition)</c> before
    /// any of those (only the rows that match are aggregated), <c>g.Select(x => ...).Distinct().Count()</c>
    /// and <c>g.Any(condition)</c>. <see cref="DbFunctions"/> lists the functions.
    /// </para>
    /// <para>
    /// Aggregates follow SQL, not LINQ: they skip nulls, a distinct count does not count null, and an
    /// aggregate over no rows is null, read back as the member type's default (0 for a non-nullable number,
    /// null for a nullable one). A comparison with a value follows C# (<c>x != "a"</c> keeps a null x); one
    /// between two columns follows SQL. Anything in the result that cannot be translated, such as a lookup
    /// in a dictionary, runs in C# on the values read.
    /// </para>
    /// <para>
    /// The query's criteria apply as they do to GetList, soft-deleted rows excluded; its own sort, Top and
    /// paging must not be set (the report query has its own). Entities that span tables (inheritance) are
    /// not supported. A provider without report queries throws <see cref="NotSupportedException"/>.
    /// </para>
    /// </summary>
    public static class ReportQueryExtensions
    {
        /// <summary>The query as the source of a report, for conditions its own Where cannot take: see
        /// <see cref="IReportSource{TEntity}.Where"/>. GroupBy, Summarize and Project can also be called on the
        /// query directly.</summary>
        public static IReportSource<TEntity> Report<TEntity>(this IDatabaseQuery<TEntity> query) where TEntity : IEntity =>
            new ReportSource<TEntity>(Source(query));

        /// <summary>Groups the rows by the key: see <see cref="IReportSource{TEntity}.GroupBy"/>.</summary>
        public static IGroupedQuery<TEntity, TKey> GroupBy<TEntity, TKey>(this IDatabaseQuery<TEntity> query,
            Expression<Func<TEntity, TKey>> key) where TEntity : IEntity => query.Report().GroupBy(key);

        /// <summary>Aggregates every row into one: see <see cref="IReportSource{TEntity}.Summarize"/>.</summary>
        public static IReportQuery<TResult> Summarize<TEntity, TResult>(this IDatabaseQuery<TEntity> query,
            Expression<Func<IEnumerable<TEntity>, TResult>> selector) where TEntity : IEntity => query.Report().Summarize(selector);

        /// <summary>Reads values from each row: see <see cref="IReportSource{TEntity}.Project"/>.</summary>
        public static IReportQuery<TResult> Project<TEntity, TResult>(this IDatabaseQuery<TEntity> query,
            Expression<Func<TEntity, TResult>> selector) where TEntity : IEntity => query.Report().Project(selector);

        static DatabaseQuery Source(IDatabaseQuery query)
        {
            var result = query as DatabaseQuery ??
                throw new NotSupportedException($"{query?.GetType().Name} does not support report queries.");

            if (result.TakeTop.HasValue || result.PageSize.HasValue || result.OrderByParts.Any())
                throw new ArgumentException("A report query takes its sort, Top and paging from its own methods, not from the query it reads.");

            if (result.Include.Any())
                throw new ArgumentException("A report query reads associations through its lambdas; Include() does not apply.");

            return result;
        }
    }

    /// <summary>The rows a report reads: the query's own, narrowed by conditions written as report lambdas.</summary>
    public interface IReportSource<TEntity> where TEntity : IEntity
    {
        /// <summary>Keeps only the rows that meet the condition (SQL's WHERE), translated as the report's other
        /// lambdas are: for a condition the query's own Where cannot take, such as one on a computed text, on a
        /// function, or through several associations.</summary>
        IReportSource<TEntity> Where(Expression<Func<TEntity, bool>> condition);

        /// <summary>Groups the rows by the key: one row per distinct key, in which aggregates are computed.
        /// An anonymous type groups by each of its members.</summary>
        IGroupedQuery<TEntity, TKey> GroupBy<TKey>(Expression<Func<TEntity, TKey>> key);

        /// <summary>Aggregates every row into one result row, such as several totals read at once. The
        /// lambda's parameter is the rows.</summary>
        IReportQuery<TResult> Summarize<TResult>(Expression<Func<IEnumerable<TEntity>, TResult>> selector);

        /// <summary>Reads the values the selector picks from each row, including through associations. Add
        /// <see cref="IReportQuery{TResult}.Distinct"/> for each distinct row once.</summary>
        IReportQuery<TResult> Project<TResult>(Expression<Func<TEntity, TResult>> selector);
    }

    /// <summary>A report query grouped by a key, waiting for what to read from each group.</summary>
    public interface IGroupedQuery<TEntity, TKey> where TEntity : IEntity
    {
        /// <summary>Keeps only the groups that meet the condition, which may use aggregates (SQL's HAVING).</summary>
        IGroupedQuery<TEntity, TKey> Having(Expression<Func<IGrouping<TKey, TEntity>, bool>> condition);

        /// <summary>What each group's row holds: its key or key members, and aggregates of its rows.</summary>
        IReportQuery<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, TEntity>, TResult>> selector);
    }

    /// <summary>A report query ready to run. See <see cref="ReportQueryExtensions"/>.</summary>
    public interface IReportQuery<TResult>
    {
        /// <summary>Sorts by a member of the result that the database computes. Called again, sorts by the
        /// next one within the previous.</summary>
        IReportQuery<TResult> OrderBy<TValue>(Expression<Func<TResult, TValue>> member);

        /// <summary>Sorts by a member of the result, largest first. See <see cref="OrderBy"/>.</summary>
        IReportQuery<TResult> OrderByDescending<TValue>(Expression<Func<TResult, TValue>> member);

        /// <summary>Keeps only the result rows that meet the condition, written on the result's members: for a
        /// projection it filters the rows read (SQL's WHERE), for a group or a summary the groups (HAVING). Only
        /// members the database computes can be used, not ones worked out in C#.</summary>
        IReportQuery<TResult> Where(Expression<Func<TResult, bool>> condition);

        /// <summary>Reads at most this many rows, in the sort order.</summary>
        IReportQuery<TResult> Top(int rows);

        /// <summary>Reads one page of rows, in the sort order.</summary>
        IReportQuery<TResult> Page(int startIndex, int pageSize);

        /// <summary>Reads each distinct row once.</summary>
        IReportQuery<TResult> Distinct();

        /// <summary>The command timeout for this query. A timeout throws the provider's own exception.</summary>
        IReportQuery<TResult> Timeout(TimeSpan timeout);

        /// <summary>Runs the query.</summary>
        Task<List<TResult>> GetList();

        /// <summary>Runs the query, and with its rows how many it yields in all (as <see cref="Count"/>), in the
        /// one statement: for a page and the pager's total at the price of one query.</summary>
        Task<(List<TResult> Rows, int Total)> GetListWithTotal();

        /// <summary>Runs the query and returns its first row, or the default when there is none.</summary>
        Task<TResult> FirstOrDefault();

        /// <summary>How many rows the query yields (groups, for a grouped query), ignoring Top and paging.</summary>
        Task<int> Count();
    }

    enum ReportKind { Grouping, Summary, Projection }

    class ReportSource<TEntity> : IReportSource<TEntity> where TEntity : IEntity
    {
        readonly DatabaseQuery Query;
        readonly List<LambdaExpression> RowConditions = new List<LambdaExpression>();

        public ReportSource(DatabaseQuery query) => Query = query;

        public IReportSource<TEntity> Where(Expression<Func<TEntity, bool>> condition)
        {
            RowConditions.Add(condition ?? throw new ArgumentNullException(nameof(condition)));
            return this;
        }

        public IGroupedQuery<TEntity, TKey> GroupBy<TKey>(Expression<Func<TEntity, TKey>> key) =>
            new GroupedQuery<TEntity, TKey>(Query, key ?? throw new ArgumentNullException(nameof(key)), RowConditions);

        public IReportQuery<TResult> Summarize<TResult>(Expression<Func<IEnumerable<TEntity>, TResult>> selector) =>
            new ReportQuery<TResult>(Query, ReportKind.Summary, null, selector ?? throw new ArgumentNullException(nameof(selector)), null, RowConditions);

        public IReportQuery<TResult> Project<TResult>(Expression<Func<TEntity, TResult>> selector) =>
            new ReportQuery<TResult>(Query, ReportKind.Projection, null, selector ?? throw new ArgumentNullException(nameof(selector)), null, RowConditions);
    }

    class GroupedQuery<TEntity, TKey> : IGroupedQuery<TEntity, TKey> where TEntity : IEntity
    {
        readonly DatabaseQuery Source;
        readonly LambdaExpression Key;
        readonly List<LambdaExpression> Conditions = new List<LambdaExpression>();
        readonly List<LambdaExpression> RowConditions;

        public GroupedQuery(DatabaseQuery source, LambdaExpression key, IEnumerable<LambdaExpression> rowConditions)
        {
            Source = source;
            Key = key;
            RowConditions = rowConditions.ToList();
        }

        public IGroupedQuery<TEntity, TKey> Having(Expression<Func<IGrouping<TKey, TEntity>, bool>> condition)
        {
            Conditions.Add(condition ?? throw new ArgumentNullException(nameof(condition)));
            return this;
        }

        public IReportQuery<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, TEntity>, TResult>> selector) =>
            new ReportQuery<TResult>(Source, ReportKind.Grouping, Key, selector ?? throw new ArgumentNullException(nameof(selector)), Conditions, RowConditions);
    }

    class ReportQuery<TResult> : IReportQuery<TResult>
    {
        readonly DatabaseQuery Source;
        readonly ReportKind Kind;
        readonly LambdaExpression Key, Selector;
        readonly LambdaExpression[] Conditions;
        readonly List<(LambdaExpression Member, bool Descending)> Sort = new List<(LambdaExpression, bool)>();
        readonly List<LambdaExpression> Filters = new List<LambdaExpression>();
        int? TakeTop, PageSize;
        int PageStartIndex;
        bool IsDistinct;
        TimeSpan? CommandTimeout;

        readonly LambdaExpression[] RowConditions;

        public ReportQuery(DatabaseQuery source, ReportKind kind, LambdaExpression key, LambdaExpression selector,
            IEnumerable<LambdaExpression> conditions, IEnumerable<LambdaExpression> rowConditions = null)
        {
            RowConditions = rowConditions?.ToArray() ?? new LambdaExpression[0];
            Source = source;
            Kind = kind;
            Key = key;
            Selector = selector;
            Conditions = conditions?.ToArray() ?? new LambdaExpression[0];
        }

        public IReportQuery<TResult> OrderBy<TValue>(Expression<Func<TResult, TValue>> member)
        {
            Sort.Add((member ?? throw new ArgumentNullException(nameof(member)), false));
            return this;
        }

        public IReportQuery<TResult> OrderByDescending<TValue>(Expression<Func<TResult, TValue>> member)
        {
            Sort.Add((member ?? throw new ArgumentNullException(nameof(member)), true));
            return this;
        }

        public IReportQuery<TResult> Where(Expression<Func<TResult, bool>> condition)
        {
            Filters.Add(condition ?? throw new ArgumentNullException(nameof(condition)));
            return this;
        }

        public IReportQuery<TResult> Top(int rows)
        {
            TakeTop = rows;
            return this;
        }

        public IReportQuery<TResult> Page(int startIndex, int pageSize)
        {
            PageStartIndex = startIndex;
            PageSize = pageSize;
            return this;
        }

        public IReportQuery<TResult> Distinct()
        {
            IsDistinct = true;
            return this;
        }

        public IReportQuery<TResult> Timeout(TimeSpan timeout)
        {
            CommandTimeout = timeout;
            return this;
        }

        ReportTranslation<TResult> Translate(int? top, bool withTotal = false) =>
            new ReportTranslator(Source).Translate<TResult>(Kind, Key, Selector, RowConditions, Conditions, Filters, IsDistinct, Sort, top,
                PageStartIndex, PageSize, withTotal);

        async Task<List<TResult>> Read(int? top)
        {
            var translation = Translate(top);
            var result = new List<TResult>();

            await translation.Run(CommandTimeout, row => result.Add(translation.Materialize(row)));
            return result;
        }

        public async Task<(List<TResult> Rows, int Total)> GetListWithTotal()
        {
            // A window count is taken before DISTINCT, so it would count the duplicates too.
            if (IsDistinct) return (await GetList(), await Count());

            var translation = Translate(TakeTop, withTotal: true);
            var rows = new List<TResult>();
            var total = 0;

            await translation.Run(CommandTimeout, row =>
            {
                rows.Add(translation.Materialize(row));
                total = Convert.ToInt32(row[translation.TotalColumn.Value]);
            });

            return (rows, total);
        }

        public Task<List<TResult>> GetList() => Read(TakeTop);

        public async Task<TResult> FirstOrDefault() =>
            (await Read(Kind == ReportKind.Summary || PageSize.HasValue ? TakeTop : TakeTop ?? 1)).FirstOrDefault();

        public async Task<int> Count()
        {
            var translation = Translate(TakeTop);
            var count = 0;

            await translation.RunCount(CommandTimeout, value => count = Convert.ToInt32(value));
            return count;
        }
    }
}
