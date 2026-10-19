# Direct operations

Direct operations are statements that go **straight to the database, outside Olive's entity pipeline**. One statement handles every matching row, instead of loading the rows and saving or deleting each one. Use them for volume (a retention sweep, a nightly rollup, millions of imported rows) and for concurrency (claiming work from a queue table).

They are named `Direct...` so that every call site says it: **this is outside Olive's scope**.

```csharp
// Delete in batches, passing over rows another transaction holds:
await Database.Of<LogMessage>().Where(x => x.Date < cutoff).Top(5000).SkipLocked().DirectDelete();

// Update in one statement:
await Database.Of<LogInboxItem>().Where(x => x.Attempts >= 5)
    .DirectUpdate(set => set.Set(x => x.Attempts, 0).Set(x => x.LeasedUntil, null));

// Take work atomically: no two callers can claim the same row.
var items = await Database.Of<LogInboxItem>()
    .Where(x => x.LeasedUntil == null || x.LeasedUntil < now)
    .OrderBy(x => x.Received).Top(50).SkipLocked()
    .DirectClaim(set => set.Set(x => x.LeasedUntil, now.AddMinutes(5)).Increment(x => x.Attempts, 1));

// Insert by the database's bulk copy (SqlBulkCopy, PostgreSQL COPY):
await Database.DirectInsert(points);
```

## What they skip

Everything a save or delete does per entity:

- **Validation.** Values are not validated. (`DirectInsert` does not validate either, unlike `Database.BulkInsert`.)
- **Entity events:** `Validating`, `Saving`, `Saved`, `Deleting`, `Deleted`, and so `GlobalEntityEvents`.
- **M# delete rules and cascades** ("cannot be deleted because of N dependants", cascade deletes). Only the database's own foreign keys apply.
- **Data replication.** Nothing is published to subscribers: a direct change to an exposed type, or to a type an exposed type depends on, leaves the subscribers out of date. Do not use direct operations on replicated data.
- **The audit trail** and `Database.Updated`.
- **Instances already in memory** are not marked stale. Saving one later overwrites the direct change.

## What they keep

- **The cache:** the entity type is cleared from it at once, and again when the open transaction commits or rolls back.
- **The database's constraints and triggers**, as an `INSERT`, `UPDATE` or `DELETE` would have them. This includes the bulk copy.
- **The query:** its criteria (soft-deleted rows are excluded, as in `GetList`), its sort and its `Top`. A query with a `Top` affects at most that many rows, in its sort order.
- **Transactions:** they run in the open `DbTransactionScope` and roll back with it.

## The operations

| Operation | Does |
|---|---|
| `query.DirectDelete()` | Deletes the matching rows and returns how many. Refuses soft-delete entities: use `DirectUpdate` to mark them. |
| `query.DirectUpdate(set => ...)` | Updates the matching rows and returns how many. |
| `query.DirectClaim(set => ...)` | Takes the matching rows and updates them in one atomic step, then returns them as entities. |
| `Database.DirectInsert(entities)` | Inserts new entities by bulk copy, joining the open `DbTransactionScope`. Where a copy can't be used (an auto-number, a Blob, inheritance, a `System.Transactions` scope), it inserts one row at a time, still with no validation or events. |
| `query.SkipLocked()` | Makes the operation pass over rows another transaction holds instead of waiting for them. |

The `set` builder of `DirectUpdate` and `DirectClaim`:

| Method | Column becomes |
|---|---|
| `Set(x => x.P, value)` | The value. For an association, set its Id property. |
| `SetFrom(x => x.P, x => x.Q)` | Another column of the same row. |
| `Increment(x => x.P, n)` | Its own value plus `n`. |
| `SetIfEmpty(x => x.P, value)` | The value, only where it is null (or, for text, empty). |
| `SetIfEarlier(x => x.P, value)` | The value, only where it is null or later than the value. |

Every assignment is worked out against the row as it is in the database, in the statement that updates it. So two concurrent updates of the same rows can't lose each other's work, as a load-and-save would.

## Conditions on other tables

`WhereExists` and `WhereNotExists` add a correlated subquery: "has (or has no) related row". Unlike `WhereNotIn`, a NULL in the related column does not empty the result.

```csharp
// Requests left with no occurrence:
await Database.Of<LogRequest>().WhereNotExists("ID", Database.Of<LogOccurrence>(), "Request").DirectDelete();
```

## Limits

- Not supported: entities whose rows span tables (inheritance), paging (use `Top`), and `Include`.
- Only providers that implement them have them: SQL Server and PostgreSQL. Others throw `NotSupportedException`. A provider adds them by overriding `SqlCommandGenerator.GenerateDirectDeleteCommand`, `GenerateDirectUpdateCommand` and `GenerateDirectClaimCommand`, and implementing `IBulkCopyCommandGenerator`.
- **SQL Server's `SkipLocked`** (READPAST) skips only a lock it meets on the index it reads. On a heap (a table with no clustered index, which is how M# creates tables), it can still wait for a row it reaches through a lookup. PostgreSQL (SKIP LOCKED) skips every held row.
