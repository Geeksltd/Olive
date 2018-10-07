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
        static ConcurrentDictionary<Type, List<Type>> ImplementationsCache = new ConcurrentDictionary<Type, List<Type>>();

        public InterfaceDataProvider(Type interfaceType) => InterfaceType = interfaceType;

        public override Type EntityType => InterfaceType;

        List<Type> GetImplementers() => ImplementationsCache.GetOrAdd(InterfaceType, FindImplementers);

        static List<Type> FindImplementers(Type interfaceType)
        {
            var result = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.References(interfaceType.Assembly)))
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type == interfaceType) continue;
                        if (type.IsInterface) continue;

                        if (type.Implements(interfaceType))
                            result.Add(type);
                    }
                }
                catch
                {
                    // Can't load assembly. No logging is needed.
                }
            }

            // For any type, if it's parent is in the list, exclude it:

            var typesWithParentsIn = result.Where(x => result.Contains(x.BaseType)).ToArray();

            foreach (var item in typesWithParentsIn)
                result.Remove(item);

            return result;
        }

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