namespace Olive.Entities.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    partial class DatabaseQuery
    {
        bool IsCacheable()
        {
            if (PageSize.HasValue) return false;

            if (Criteria.Except(typeof(DirectDatabaseCriterion)).Any(c => c.PropertyName.Contains(".")))
                return false; // This doesn't work with cache expiration rules.

            if (Criteria.OfType<DirectDatabaseCriterion>().Any(x => !x.IsCacheSafe))
                return false;

            // Do not cache a polymorphic call:
            if (NeedsTypeResolution()) return false;

            return true;
        }

        string GetCacheKey()
        {
            var r = new StringBuilder();
            r.Append(EntityType.GetCachedAssemblyQualifiedName());

            r.Append(':');

            foreach (var c in Criteria)
            {
                r.Append(c.ToString());
                r.Append('|');
            }

            if (TakeTop.HasValue) r.Append("|N:" + TakeTop);

            r.Append(OrderByParts.Select(x => x.ToString()).ToString(",").WithPrefix("|S:"));

            return r.ToString();
        }

        public async Task<IEnumerable<IEntity>> GetList()
        {
            if (!IsCacheable()) return await LoadFromDatabase();

            var cacheKey = GetCacheKey();

            var result = Cache.GetList(EntityType, cacheKey)?.Cast<IEntity>();
            if (result != null)
            {
                await LoadIncludedAssociations(result);
                return result;
            }

            result = await LoadFromDatabaseAndCache();

            // If there is no transaction open, cache it:
            if (!Context.Current.Database().AnyOpenTransaction())
                Cache.AddList(EntityType, cacheKey, result);

            return result;
        }

        async Task<List<IEntity>> LoadFromDatabase()
        {
            List<IEntity> result;
            if (NeedsTypeResolution())
            {
                var queries = ResolveDataProviders().Select(p => p.GetList(this));
                result = await queries.SelectManyAsync(x => x).ToList();
            }
            else
                result = await Provider.GetList(this).ToList();

            if (OrderByParts.None())
            {
                // TODO: If the entity is sortable by a single DB column, then automatically add that to the DB call.
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

        async Task<List<IEntity>> LoadFromDatabaseAndCache()
        {
            var timestamp = Cache.GetQueryTimestamp();

            var result = new List<IEntity>();

            foreach (var item in await LoadFromDatabase())
            {
                var inCache = Cache.Get(item.GetType(), item.GetId().ToString());
                if (inCache != null) result.Add(inCache);
                else
                {
                    await Entity.Services.RaiseOnLoaded(item);
                    (Context.Current.Database() as Database)?.TryCache(item, timestamp);
                    result.Add(item);
                }
            }

            return result;
        }

        public Task<int> Count() => Provider.Count(this);

        public async Task<bool> Any() => await Count() > 0;

        public async Task<bool> None() => !await Any();

        public async Task<IEntity> FirstOrDefault()
        {
            TakeTop = 1;
            return (await Provider.GetList(this)).FirstOrDefault();
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        public new async Task<IEnumerable<TEntity>> GetList() => (await base.GetList()).Cast<TEntity>();

        public new async Task<TEntity> FirstOrDefault() => (TEntity)(await base.FirstOrDefault());
    }
}