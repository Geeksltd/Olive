using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides data access for Interface types.
    /// </summary>
    public class InterfaceDataProvider : IDataProvider
    {
        Type InterfaceType;
        static ConcurrentDictionary<Type, List<Type>> ImplementationsCache = new ConcurrentDictionary<Type, List<Type>>();

        public InterfaceDataProvider(Type interfaceType) => InterfaceType = interfaceType;

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
                    // Can't load assembly
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
            return implementers.Select(x => Database.Instance.GetProvider(x)).ToList();
        }

        public async Task<int> Count(IDatabaseQuery query)
        {
            var providers = FindProviders();
            var results = await providers.Select(x => x.Count(query)).AwaitAll();
            return results.Sum();
        }

        public async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            if (query.TakeTop.HasValue)
                throw new Exception("Top() criteria is not allowed when querying based on Interfaces.");

            if (((DatabaseQuery)query).OrderByParts.Any())
                throw new Exception("OrderBy() is not allowed when querying based on Interfaces.");

            var providers = FindProviders();
            var results = await providers.Select(x => x.GetList(query)).AwaitAll();
            return results.SelectMany(x => x);
        }

        public DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery query,
            PropertyInfo association)
        {
            throw new InvalidOperationException("Oops! GetAssociationInclusionCriteria() is not meant to be ever called on " + GetType().Name);
        }

        public Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName) =>
            throw new NotSupportedException("Database.Aggregate doesn't work on interfaces.");

        public async Task<IEntity> Get(object objectID)
        {
            foreach (var actual in GetImplementers())
            {
                try
                {
                    if (await Entity.Database.Get(objectID, actual) is Entity result) return result;
                }
                catch { continue; }
            }

            throw new Exception($"There is no {InterfaceType.Name} record with the ID of '{objectID}'");
        }

        public Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property) =>
            throw new NotSupportedException("IDataProvider.ReadManyToManyRelation() is not supported for Interfaces");

        public Task Save(IEntity record) =>
            throw new NotSupportedException("IDataProvider.Save() is irrelevant to Interfaces");

        public Task Delete(IEntity record) =>
            throw new NotSupportedException("IDataProvider.Delete() is irrelevant to Interfaces");

        public string MapColumn(string propertyName) =>
            throw new NotSupportedException("IDataProvider.Delete() is irrelevant to Interfaces");

        public IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated) =>
            throw new NotSupportedException("GetUpdatedValues() is irrelevant to Interfaces");

        public Task<int> ExecuteNonQuery(string command) =>
            throw new NotSupportedException("ExecuteNonQuery() is irrelevant to Interfaces");

        public Task<object> ExecuteScalar(string command) =>
            throw new NotSupportedException("ExecuteScalar() is irrelevant to Interfaces");

        public bool SupportValidationBypassing() =>
            throw new NotSupportedException("SupportValidationBypassing() is irrelevant to Interfaces");

        public Task BulkInsert(IEntity[] entities, int batchSize) =>
            throw new NotSupportedException("BulkInsert() is irrelevant to Interfaces");

        public Task BulkUpdate(IEntity[] entities, int batchSize) =>
            throw new NotSupportedException("BulkInsert() is irrelevant to Interfaces");

        public IDataAccess Access =>
            throw new NotSupportedException("Access is irrelevant to Interfaces");

        public string MapSubquery(string path) =>
            throw new NotSupportedException("MapSubquery() is irrelevant to Interfaces");

        public string ConnectionString { get; set; }

        public string ConnectionStringKey { get; set; }
    }
}