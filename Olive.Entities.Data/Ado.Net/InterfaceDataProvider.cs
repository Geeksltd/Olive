using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides data access for Interface types.
    /// </summary>
    public class InterfaceDataProvider : LimitedDataProvider
    {
        Type InterfaceType;
        static ConcurrentDictionary<Type, Type[]> ImplementationsCache = new ConcurrentDictionary<Type, Type[]>();

        public InterfaceDataProvider(Type interfaceType) => InterfaceType = interfaceType;

        public override Type EntityType => InterfaceType;

        Type[] GetImplementers() => ImplementationsCache.GetOrAdd(InterfaceType, x => AppDomain.CurrentDomain.FindImplementers(x));

        List<IDataProvider> FindProviders()
        {
            var implementers = GetImplementers();
            return implementers.Select(x => Context.Current.Database().GetProvider(x)).ToList();
        }

        public override async Task<int> Count(IDatabaseQuery query)
        {
            var providers = FindProviders();
            var results = await providers.SelectAsync(x => x.Count(query.CloneFor(x.EntityType)));
            return results.Sum();
        }

        public override async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            if (query.TakeTop.HasValue)
                throw new Exception("Top() criteria is not allowed when querying based on Interfaces.");

            if (((DatabaseQuery)query).OrderByParts.Any())
                throw new Exception("OrderBy() is not allowed when querying based on Interfaces.");

            var providers = FindProviders();
            var results = await providers.SelectAsync(x => x.GetList(query.CloneFor(x.EntityType)));
            return results.SelectMany(x => x);
        }

        public override async Task<IEntity> Get(object objectID)
        {
            foreach (var actual in GetImplementers())
            {
                try
                {
                    if (await Context.Current.Database().Get(objectID, actual) is Entity result) return result;
                }
                catch
                {
                    // No logging is needed.
                    continue;
                }
            }

            throw new Exception($"There is no {InterfaceType.Name} record with the ID of '{objectID}'");
        }
    }
}