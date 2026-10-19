# Report queries

Report queries read **groups, totals and projections that the database computes**, and return them as typed rows rather than entities. Use them for dashboards, charts and reports: anything that would otherwise be raw SQL with `GROUP BY`, or a load of every row to add them up in C#.

They are plain reads. Nothing is cached, saved or raised, and they work inside a transaction like any other query.

```csharp
var rows = await Database.Of<MetricDataPoint>()
    .Where(p => p.Definition.AccountId == accountId && p.Timestamp >= from)       // Olive's criteria, as ever
    .GroupBy(p => new { p.Definition.ServiceId, Day = p.Timestamp.Date })
    .Having(g => g.Count() >= 3)
    .Select(g => new
    {
        Service = g.Key.ServiceId,
        g.Key.Day,
        Requests = g.Where(p => p.Definition.Key == "api.requests").Sum(p => p.Value),
        Failed = g.Where(p => p.Definition.Key != "api.requests").Sum(p => p.Value),
        Points = g.Count()
    })
    .OrderByDescending(r => r.Requests).Top(10)
    .GetList();
```

## The three kinds

| Start | Returns |
|---|---|
| `query.GroupBy(x => key).Select(g => ...)` | One row per distinct key. An anonymous key groups by each of its members. |
| `query.Summarize(rows => ...)` | One row of aggregates over every matching row, such as several totals read at once. |
| `query.Project(x => ...)` | The values the selector picks from each row, including through associations. Add `.Distinct()` for each distinct row once. |

Each returns an `IReportQuery<TResult>`, which takes `OrderBy`, `OrderByDescending` (called again: the next sort key), `Top`, `Page(startIndex, pageSize)`, `Distinct`, `Timeout`, `Where` (see below), and runs with `GetList()`, `FirstOrDefault()`, `Count()` (how many rows it yields, ignoring `Top` and paging) or `GetListWithTotal()` (the rows and that count in one statement, for a page and its pager).

## Conditions

There are three places to filter, and each has its own job:

| Where | Does | Translated by |
|---|---|---|
| `query.Where(x => ...)` before the report | Filters the rows, as for `GetList` (soft-deleted rows are excluded) | Olive's criteria |
| `query.Report().Where(x => ...)` | Filters the rows, for conditions Olive's criteria cannot take: a computed text, a function, a negated `Contains`, a role test through several associations | The report translator |
| `.Having(g => ...)`, or `.Where(r => ...)` on the result | Filters the groups (HAVING); on a projection, the rows | The report translator |

```csharp
var logged = await Database.Of<LogOccurrence>().Where(o => o.Date >= from).Report()
    .Where(o => o.Log.Source.Replace(".", "").ToLower() == name)
    .GroupBy(o => o.Date.Date)
    .Select(g => new { Day = g.Key, Errors = g.Count(o => o.Log.Severity >= 4) })
    .GetList();
```

## What the lambdas can use

The lambdas are translated, not run.

- **Properties and associations.** A path such as `p.Definition.Resource.Name` joins the associated tables, one join per path. End a path with the association's Id property (`p.Definition.ServiceId`) to read the key itself. A criterion on an association path in the query's own `Where` reads the same join. A join is a LEFT JOIN, so a row with no associated row stays in (its values read as null), unless a condition at the top of the filters needs the associated row: a comparison of one of its columns with a value, or `x.Definition != null`. Then it is an INNER JOIN, which is the same rows and lets the database plan the query as it would a hand-written one.
- **Aggregates inside a group:** `g.Key` (or its members), `g.Count()`, `g.Count(condition)`, `g.Sum/Min/Max/Average(x => ...)`, `g.Where(condition)` before any of those (only the matching rows are aggregated), `g.Select(x => ...).Distinct().Count()`, and `g.Any(condition)`. In `Summarize` the same work on the `rows` parameter.
- **C# with an obvious SQL meaning:** `x.Date` (the day), `x.Hour`, `x.DayOfWeek` (Sunday is 0), `Trim()`, `ToLower()`, `ToUpper()`, `Replace()`, `Contains()` (optionally with `StringComparison.OrdinalIgnoreCase`), `StartsWith()`, `EndsWith()`, `string.IsNullOrEmpty()`, string `+`, `??`, `?:`, arithmetic, casts to `double`, `Guid.ToString()`, and `list.Contains(x)` or `x.IsAnyOf(list)`.
- **`DbFunctions`:** `TruncateToHour`, `TruncateToMinutes(x, n)`, `Left(x, n)`, `NullIfEmpty`, `CaseInsensitive`, `Like(x, pattern)` and `LikeAny(x, patterns)` (with `ESCAPE '\'`).
- **Values the lambda captures** are worked out in C# first. A captured flag picks a branch before translation: `daily ? x.Timestamp.Date : x.Timestamp` groups by one or the other, not by a SQL `CASE`.
- **Anything in the result that has no translation** (a dictionary lookup, a format, a constructor) runs in C# on the values read. Only `Where`, `Having` and sorting need everything they use to be translatable.

## How the results follow SQL

- Aggregates skip nulls, and a distinct count does not count null.
- An aggregate over no rows is null. It is read back as the member's default: 0 for a non-nullable number, null for a nullable one. Select a nullable type (`g.Max(x => (DateTime?)x.Date)`) to tell "none" from 0.
- A comparison with a value follows C#: `x != "a"` keeps a null `x`. A comparison between two columns follows SQL.
- A division follows the database's arithmetic: `g.Sum(x => x.Amount) / g.Count()` divides in the column's own type, as SQL written by hand would, so a decimal column gives a decimal result (SQL Server keeps six decimal places of it), not the floating point C#'s cast of the count would give.
- Rows that tie on the sort come back in whatever order the database chooses, as in any SQL. Sort by a unique column last when the order matters, for example for paging.

## Limits

- Not supported: entities whose rows span tables (inheritance), associations in another database, and a sort, `Top`, paging or `Include` on the query itself (the report query has its own).
- Only providers that implement them have them: SQL Server and PostgreSQL. Others throw `NotSupportedException`. A provider adds them by overriding `SqlCommandGenerator.GenerateReportCommand` and the functions it lists (`TruncateToHour`, `TruncateToDay`, `TruncateToMinutes`, `DayOfWeek`, `HourOfDay`, `IndexOf`, `CaseInsensitive`, `ToDouble`, `GuidAsText`).
