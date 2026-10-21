# Set-based writes

Set-based writes delete, update or insert every row a query matches (or every entity given) in one call:

```csharp
await Database.Of<Order>().Where(x => x.Status == "Abandoned").Delete();
await Database.Of<Order>().Where(x => x.Due < today).Update(set => set.Set(x => x.Status, "Overdue"));
await Database.Insert(orders);
```

They replace `Database.DeleteAll<T>()` and `Database.UpdateAll<T>()`, which are now obsolete.

Each takes an optional `bypassEntityPipeline` argument, `false` by default:

- **`false` (the default): through Olive's entity pipeline.** The matching entities are loaded, then deleted by `Database.Delete`, changed and saved by `Database.Update`, or saved by `Database.Save`, all in one transaction. Everything a save or delete does happens: validation, the entity events, M#'s delete rules and cascades, soft delete, data replication, the audit trail and `Database.Updated`.
- **`true`: one statement for every matching row, outside Olive's entity pipeline.** Use it for volume (a retention sweep, a nightly rollup, millions of imported rows) and concurrency. The name says what it costs at every call site: **none of the pipeline runs.**

```csharp
// Delete in batches, passing over rows another transaction holds:
await Database.Of<LogMessage>().Where(x => x.Date < cutoff).Top(5000).SkipLocked()
    .Delete(bypassEntityPipeline: true);

// Update in one statement:
await Database.Of<LogInboxItem>().Where(x => x.Attempts >= 5)
    .Update(set => set.Set(x => x.Attempts, 0).Set(x => x.LeasedUntil, null), bypassEntityPipeline: true);

// Take work atomically: no two callers can claim the same row. A claim always bypasses the pipeline.
var items = await Database.Of<LogInboxItem>()
    .Where(x => x.LeasedUntil == null || x.LeasedUntil < now)
    .OrderBy(x => x.Received).Top(50).SkipLocked()
    .Claim(set => set.Set(x => x.LeasedUntil, now.AddMinutes(5)).Increment(x => x.Attempts, 1));

// Insert by the database's bulk copy (SqlBulkCopy, PostgreSQL COPY):
await Database.Insert(points, bypassEntityPipeline: true);
```

## The operations

| Operation | Through the pipeline (default) | `bypassEntityPipeline: true` |
|---|---|---|
| `query.Delete()` | Loads the matching entities and deletes each with `Database.Delete`. A soft-delete entity is marked. Returns how many. | One `DELETE`. Refuses soft-delete entities: mark them with `Update`. Returns how many. |
| `query.Update(set => ...)` | Loads the matching entities, makes the change to each in memory and saves it with `Database.Update`. Returns how many. | One `UPDATE`, every value computed from the row as it is in the database. Returns how many. |
| `query.Claim(set => ...)` | — | Always: takes the matching rows and updates them in one atomic step, then returns them as entities. Loading and saving could not be atomic, so there is no pipeline version. |
| `Database.Insert(entities)` | Saves each new entity with `Database.Save`. | Bulk copy, joining the open `DbTransactionScope`. Where a copy can't be used (an auto-number, a Blob, inheritance, a `System.Transactions` scope), one `INSERT` per row, still outside the pipeline. |
| `query.SkipLocked()` | Refused (`ArgumentException`). | Passes over rows another transaction holds instead of waiting for them. |

The `set` builder of `Update` and `Claim`:

| Method | Column becomes |
|---|---|
| `Set(x => x.P, value)` | The value. For an association, set its Id property. |
| `SetFrom(x => x.P, x => x.Q)` | Another column of the same row. |
| `Increment(x => x.P, n)` | Its own value plus `n`. |
| `SetIfEmpty(x => x.P, value)` | The value, only where it is null (or, for text, empty). |
| `SetIfEarlier(x => x.P, value)` | The value, only where it is null or later than the value. |

Through the pipeline, an `Update` is a load-and-save: another caller's change to the same row between the load and the save is overwritten. With `bypassEntityPipeline: true` every assignment is worked out against the row in the statement that updates it, so two concurrent updates can't lose each other's work.

Either way the query's criteria (soft-deleted rows are excluded, as in `GetList`), its sort and its `Top` apply: a query with a `Top` affects at most that many rows, in its sort order. Paging does not: use `Top`.

## What `bypassEntityPipeline: true` skips

Everything a save or delete does per entity:

- **Validation.** Values are not validated. (`Database.Insert(..., bypassEntityPipeline: true)` does not validate either, unlike `Database.BulkInsert`.)
- **Entity events:** `Validating`, `Saving`, `Saved`, `Deleting`, `Deleted`, and so `GlobalEntityEvents`.
- **M# delete rules and cascades** ("cannot be deleted because of N dependants", cascade deletes). Only the database's own foreign keys apply.
- **Data replication.** Nothing is published to subscribers: a change to an exposed type, or to a type an exposed type depends on, leaves the subscribers out of date. Do not bypass the pipeline on replicated data.
- **The audit trail** and `Database.Updated`.
- **Instances already in memory** are not marked stale. Saving one later overwrites the change.

## What it keeps

- **The cache:** the entity type is cleared from it at once, and again when the open transaction commits or rolls back.
- **The database's constraints and triggers**, as an `INSERT`, `UPDATE` or `DELETE` would have them. This includes the bulk copy.
- **Transactions:** the statements run in the open `DbTransactionScope` and roll back with it.

## Conditions on other tables

`WhereExists` and `WhereNotExists` add a correlated subquery: "has (or has no) related row". Unlike `WhereNotIn`, a NULL in the related column does not empty the result.

```csharp
// Requests left with no occurrence:
await Database.Of<LogRequest>().WhereNotExists("ID", Database.Of<LogOccurrence>(), "Request")
    .Delete(bypassEntityPipeline: true);
```

## Limits of `bypassEntityPipeline: true` and `Claim`

- Not supported: entities whose rows span tables (inheritance) and `Include`. Through the pipeline, inheritance works.
- Only providers that implement the statements have them: SQL Server and PostgreSQL. Others throw `NotSupportedException`. A provider adds them by overriding `SqlCommandGenerator.GenerateDirectDeleteCommand`, `GenerateDirectUpdateCommand` and `GenerateDirectClaimCommand`, and implementing `IBulkCopyCommandGenerator`.
- **SQL Server's `SkipLocked`** (READPAST) skips only a lock it meets on the index it reads. On a heap (a table with no clustered index, which is how M# creates tables), it can still wait for a row it reaches through a lookup. PostgreSQL (SKIP LOCKED) skips every held row.
- **SQL Server's `SkipLocked` at any isolation level:** a bare READPAST is refused at SERIALIZABLE or SNAPSHOT, and a pooled connection keeps the level of the last transaction it ran. So `Delete` and `Update` add READCOMMITTEDLOCK, which reads the table at READ COMMITTED whatever the session is at. `Claim` cannot (with UPDLOCK, SQL Server still refuses READPAST there), so a claim with `SkipLocked` needs the session at READ COMMITTED or REPEATABLE READ.
