namespace Olive.Entities.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    partial class DatabaseQuery
    {
        static IEnumerable<ICriterion> Flatten(IEnumerable<ICriterion> criteria)
        {
            foreach (var criterion in criteria)
            {
                if (criterion is BinaryCriterion binary)
                {
                    foreach (var inner in Flatten(new ICriterion[] { binary.Left, binary.Right }.ExceptNull()))
                        yield return inner;
                }
                else yield return criterion;
            }
        }

        bool IsCacheable()
        {
            if ((TakeTop.HasValue && TakeTop != 1) || PageSize.HasValue || Columns.Any()) return false;

            // The soft-delete bypass context changes the generated SQL but is invisible to cache keys.
            if (SoftDeleteAttribute.Context.ShouldByPassSoftDelete()) return false;

            // We don't cache queries that include associations, because the included associations may change without changing the main entity.
            if (Include.Any()) return false;

            var criteria = Flatten(Criteria).ToArray();

            if (criteria.Except(typeof(DirectDatabaseCriterion)).Any(c => c.PropertyName.Contains(".")))
                return false; // This doesn't work with cache expiration rules.

            if (criteria.OfType<DirectDatabaseCriterion>().Any(x => !x.IsCacheSafe))
                return false;

            // Do not cache a polymorphic call:
            if (NeedsTypeResolution()) return false;

            return true;
        }

        string GetCriteriaCacheKey(string prefix = null) =>
            prefix + Criteria.Select(c => c.ToString()).OrderBy(x => x).ToString("|");

        string GetQueryCacheKey() =>
            GetCriteriaCacheKey() + "##" + OrderByParts.Select(o => o.ToString()).ToString(",") +
            "##top:" + TakeTop;

        public async Task<IEntity[]> GetList()
        {
            if (!IsCacheable()) return await LoadFromDatabase().ToArray();

            if (Context.Current.Database().AnyOpenTransaction())
                return (await LoadFromDatabaseAndCache()).Items.ToArray();

            if (Criteria.Any() || TakeTop.HasValue)
            {
                var cacheKey = GetQueryCacheKey();
                var queryCache = Cache as Cache;

                if (queryCache?.GetQueryResult(EntityType, cacheKey) is IEntity[] cached)
                    return cached.ToArray();

                var timestamp = Cache.GetQueryTimestamp();
                var queried = (await LoadFromDatabaseAndCache()).Items.ToArray();
                queryCache?.AddQueryResult(EntityType, cacheKey, queried, timestamp);
                return queried;
            }

            var result = Cache.GetList(EntityType)?.Cast<IEntity>().ToArray();
            if (result != null) return result;

            var loaded = await LoadFromDatabaseAndCache();
            result = loaded.Items.ToArray();

            // A record that changed while being loaded is not cached, so the list containing it isn't either.
            // Checked again here, for a record saved or deleted after it was cached but before the list is.
            if (loaded.AllCached && !IsAnyUpdatedSince(result, loaded.QueryTime))
                Cache.AddList(EntityType, result);

            return result;
        }

        async Task<List<IEntity>> LoadFromDatabase()
        {
            List<IEntity> result;
            if (NeedsTypeResolution())
            {
                var queries = EntityFinder.FindPossibleTypes(EntityType, mustFind: true)
                    .Select(t => CloneFor(t))
                    .Select(q => q.Provider.GetList(q));

                result = await queries.SelectManyAsync(x => x).ToList();
            }
            else
                result = await Provider.GetList(this).ToList();

            foreach (var item in result)
                await Entity.Services.RaiseOnLoaded(item);

            if (OrderByParts.None() && !SkipAutoSortAttribute.HasAttribute(EntityType))
            {
                if (EntityType.Implements<ISortable>())
                    result = result.OrderBy(x => (x as ISortable).Order).ToList();
                else
                    result.Sort();
            }

            await LoadIncludedAssociations(result);

            return result;
        }

        async Task LoadIncludedAssociations(IEnumerable<IEntity> mainResult)
        {
            foreach (var associationHeirarchy in Include)
                await associationHeirarchy.LoadAssociations(this, mainResult);
            // await new AssociationEagerLoadService(mainResult, associationHeirarchy.Association, associationHeirarchy.SubAssociations, this).Run();
        }

        bool IsAnyUpdatedSince(IEnumerable<IEntity> items, DateTime? queryTime) =>
            queryTime.HasValue && items.Any(x => Cache.IsUpdatedSince(x, queryTime.Value));

        async Task<(List<IEntity> Items, bool AllCached, DateTime? QueryTime)> LoadFromDatabaseAndCache()
        {
            var timestamp = Cache.GetQueryTimestamp();

            var result = new List<IEntity>();
            var allCached = true;

            foreach (var item in await LoadFromDatabase())
            {
                var inCache = Cache.Get(item.GetType(), item.GetId().ToString());
                if (inCache != null) result.Add(inCache);
                else
                {
                    if ((Context.Current.Database() as Database)?.TryCache(item, timestamp) == false)
                        allCached = false;

                    result.Add(item);
                }
            }

            return (result, allCached, timestamp);
        }

        public async Task<int> Count()
        {
            if (!IsCacheable() || Context.Current.Database().AnyOpenTransaction())
                return await Provider.Count(this);

            var cacheKey = GetCriteriaCacheKey("count:");
            var queryCache = Cache as Cache;

            if (queryCache?.GetQueryResult(EntityType, cacheKey) is int cached) return cached;

            var timestamp = Cache.GetQueryTimestamp();
            var count = await Provider.Count(this);
            queryCache?.AddQueryResult(EntityType, cacheKey, count, timestamp);
            return count;
        }

        public async Task<bool> Any()
        {
            if (!IsCacheable() || Context.Current.Database().AnyOpenTransaction())
                return await ExecuteAny();

            var cacheKey = GetCriteriaCacheKey("any:");
            var queryCache = Cache as Cache;

            if (queryCache?.GetQueryResult(EntityType, cacheKey) is bool cached) return cached;

            var timestamp = Cache.GetQueryTimestamp();
            var result = await ExecuteAny();
            queryCache?.AddQueryResult(EntityType, cacheKey, result, timestamp);
            return result;
        }

        async Task<bool> ExecuteAny() =>
            Provider is DataProvider adoProvider ? await adoProvider.Any(this) : await Provider.Count(this) > 0;

        public async Task<bool> None() => !await Any();

        public async Task<IEntity> FirstOrDefault()
        {
            TakeTop = 1;
            return await GetList().FirstOrDefault();
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        public new async Task<TEntity[]> GetList() => (await base.GetList()).Cast<TEntity>().ToArray();

        public new async Task<TEntity> FirstOrDefault() => (TEntity)(await base.FirstOrDefault());
    }
}