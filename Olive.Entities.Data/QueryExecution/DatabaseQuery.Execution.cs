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
            if (TakeTop.HasValue || PageSize.HasValue) return false;

            if (Criteria.Except(typeof(DirectDatabaseCriterion)).Any(c => c.PropertyName.Contains(".")))
                return false; // This doesn't work with cache expiration rules.

            if (Criteria.OfType<DirectDatabaseCriterion>().Any(x => !x.IsCacheSafe))
                return false;

            // Do not cache a polymorphic call:
            if (NeedsTypeResolution()) return false;

            return true;
        }

        public async Task<IEnumerable<IEntity>> GetList()
        {
            if (!IsCacheable()) return await LoadFromDatabase();

            if (Criteria.Any() || Context.Current.Database().AnyOpenTransaction())
                return await LoadFromDatabaseAndCache();

            var result = Cache.GetList(EntityType)?.Cast<IEntity>();
            if (result != null)
            {
                await LoadIncludedAssociations(result);
                return result;
            }

            result = await LoadFromDatabaseAndCache();

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
            return await GetList().FirstOrDefault();
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        public new async Task<IEnumerable<TEntity>> GetList() => (await base.GetList()).Cast<TEntity>();

        public new async Task<TEntity> FirstOrDefault() => (TEntity)(await base.FirstOrDefault());
    }
}