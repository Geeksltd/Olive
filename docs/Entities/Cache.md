
# Caching

Olive's ORM has a built-in cache for entities, sitting between `IDatabase` and the database itself. It reduces database round-trips for repeated reads, and is kept consistent automatically: any `Save`/`Delete` invalidates exactly what it needs to.

## What gets cached automatically

- **Single entity lookups** — `Database.Get<T>(id)` and `Database.GetOrDefault<T>(id)`.
- **Unfiltered list queries** — `Database.GetList<T>()` with no criteria.
- **Filtered list queries, counts and `FirstOrDefault`** — `Database.Of<T>().Where(...).GetList()`, `.Count()`, and `.FirstOrDefault()` (including the convenience overloads `Database.GetList<T>(criteria)`, `Database.Count<T>(criteria)`, `Database.FirstOrDefault<T>(criteria)`).

You don't need to change your code to benefit from this — if a type is cacheable (see below), matching queries are served from memory instead of hitting the database again.

A filtered query is **not** cached when it:
- Uses paging (`Top(n)` for `n > 1`, or `PageSize`) — `FirstOrDefault()`/`Top(1)` is the one exception, and *is* cached.
- Selects specific columns (`Select(...)`).
- Uses `.Include(...)` to eager-load associations (a cached result would bake in whichever `Include` shape was requested first, so these always hit the database).
- Filters on a dotted/association-traversing property (e.g. `x.Customer.Country == "UK"`) — only direct properties of the queried type are safe to cache.
- Needs polymorphic type resolution.

## Enabling caching for a type

Caching is controlled per type with the `[CacheObjects]` attribute:

```csharp
[CacheObjects(true)]
public class Product : GuidEntity { ... }
```

If a type has no `[CacheObjects]` attribute, it falls back to the global `Database:Cache` setting (see below). `[CacheObjects(false)]` always opts a type out, regardless of the global setting.

## Cache modes

Configured via `Database:Cache:Mode` in `appsettings.json`:

```json
"Database": {
    "Cache": {
        "Mode": "single-server"
    }
}
```

- `off` — caching disabled.
- `single-server` — the traditional static in-process cache, shared across all requests for the lifetime of the app. Ideal when your app runs as a single instance (vertical scaling).
- `multi-server` — the cache is scoped to a single HTTP request. Use this instead of `off` when your app is horizontally scaled across multiple instances/pods behind a load balancer: an in-process cache can't stay coherent across instances, so scoping it to one request avoids ever serving stale cross-instance data, while still deduplicating repeated loads within that one request.

## Distributed caching (Redis)

For a genuinely shared cache across multiple instances, add the `Olive.Entities.Cache.Redis` package and call:

```csharp
services.AddRedisCache();
```

This swaps in a Redis-backed `ICacheProvider` behind the same `ICache` interface, so all existing invalidation (on Save/Delete) keeps working unchanged. Two things to be aware of before using it:
- It disables the concurrency-aware staleness check that the in-process provider has (`ConcurrencyAware` has no effect under Redis).
- Filtered-query-result caching (above) is **not** available under Redis yet — only the in-process provider (`InMemoryCacheProvider`) implements it. Queries with criteria will still hit the database when Redis is the active provider.

## Growth control for filtered-query caching

Because a filtered query's cache key includes its criteria, a type queried with many different filter combinations (e.g. a search screen with several optional fields) can accumulate many distinct cache entries. To bound this, the in-process provider caps how many distinct query results it keeps per type:

```json
"Database": {
    "Cache": {
        "MaxCachedQueriesPerType": 200
    }
}
```

Once a type's cache hits this many entries, the bucket is cleared before the next one is added. The default is `200`; raise it for types with many legitimate filter combinations and a low write rate, or lower it for memory-constrained deployments.

## Invalidation

Any `Save` or `Delete` of an entity invalidates **all** cached data for that type — the single entity, the unfiltered list, and every cached filtered query result. This is intentionally coarse (not per-row or per-query-shape) to keep the guarantee simple: after any write to a type, the next read of that type is always fresh.

## Manually clearing the cache

```csharp
await Context.Current.Database().Refresh();
```

This clears the entire cache (all types) and raises `IDatabase.CacheRefreshed`.
