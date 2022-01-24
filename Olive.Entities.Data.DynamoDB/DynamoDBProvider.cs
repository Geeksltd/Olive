using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    class DynamoDBProvider<T> : IDataProvider
    {
        public IDataAccess Access => throw new NotImplementedException();

        public Type EntityType => typeof(T);

        public string ConnectionString
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string ConnectionStringKey
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName)
        {
            throw new NotImplementedException();
        }

        public Task BulkInsert(IEntity[] entities, int batchSize)
        {
            throw new NotImplementedException();
        }

        public Task BulkUpdate(IEntity[] entities, int batchSize)
        {
            throw new NotImplementedException();
        }

        public Task<int> Count(IDatabaseQuery query) => throw new NotImplementedException();

        public Task Delete(IEntity record) => Dynamo.Db.DeleteAsync<T>(record);

        public Task<int> ExecuteNonQuery(string command) => throw new NotImplementedException();

        public Task<object> ExecuteScalar(string command) => throw new NotImplementedException();

        public string GenerateSelectCommand(IDatabaseQuery iquery, string fields)
        {
            throw new NotImplementedException();
        }

        public async Task<IEntity> Get(object objectID)
        {
            return (IEntity)await Dynamo.Table<T>().Get(objectID);
        }

        public DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery masterQuery, PropertyInfo association)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            (bool IsHashKey, object Value) IsHashKey(ICriterion criterion)
            {
                var prop = EntityType.GetProperty(criterion.PropertyName);
                if (prop is null) return (false, null);

                var attr = prop.GetCustomAttribute<DynamoDBHashKeyAttribute>();
                if (attr is null) return (false, null);

                return (true, criterion.Value);
            }

            (bool IsIndex, string Name, object Value) IsIndex(ICriterion criterion)
            {
                var prop = EntityType.GetProperty(criterion.PropertyName);
                if (prop is null) return (false, null, null);

                var indexAttr = prop.GetCustomAttribute<DynamoDBGlobalSecondaryIndexHashKeyAttribute>();
                if (indexAttr is null) return (false, null, null);

                return (true, indexAttr.IndexNames.FirstOrDefault(), criterion.Value);
            }

            var hashKeyInfo = query.Criteria.Select(IsHashKey).ExceptNull().FirstOrDefault();
            if (hashKeyInfo.IsHashKey)
            {
                if (hashKeyInfo.Value is null) return Enumerable.Empty<IEntity>();
                return new[] { await Get(hashKeyInfo.Value) };
            }

            var indexInfo = query.Criteria.Select(IsIndex).ExceptNull().FirstOrDefault();
            if (indexInfo.IsIndex)
            {
                if (indexInfo.Value is null) return Enumerable.Empty<IEntity>();
                return (await Dynamo.Index<T>(indexInfo.Name).All(indexInfo.Value)).Cast<IEntity>();
            }

            return Enumerable.Empty<IEntity>();
        }

        public IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated)
        {
            throw new NotImplementedException();
        }

        public string MapColumn(string propertyName)
        {
            throw new NotImplementedException();
        }

        public string MapSubquery(string path, string parent)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new NotImplementedException();
        }

        public Task Save(IEntity record) => Dynamo.Db.SaveAsync((T)record);

        public bool SupportValidationBypassing() => throw new NotImplementedException();
    }
}