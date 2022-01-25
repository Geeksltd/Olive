using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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
            var writer = Dynamo.Db.CreateBatchWrite<T>();
            writer.AddPutItems(entities.Cast<T>());
            return writer.ExecuteAsync();
        }

        public Task BulkUpdate(IEntity[] entities, int batchSize)
        {
            var writer = Dynamo.Db.CreateBatchWrite<T>();
            entities.Do(x => writer.AddDeleteKey(x.GetId()));
            writer.AddPutItems(entities.Cast<T>());
            return writer.ExecuteAsync();
        }

        public Task<int> Count(IDatabaseQuery query) => GetList(query).Count();

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
                if (attr?.GetType() != typeof(DynamoDBHashKeyAttribute)) return (false, null);

                return (true, criterion.Value);
            }

            var hashKeyInfo = query.Criteria.Select(IsHashKey).ExceptNull().FirstOrDefault();
            if (hashKeyInfo.IsHashKey)
            {
                if (hashKeyInfo.Value is null) return Enumerable.Empty<IEntity>();
                var item = await Get(hashKeyInfo.Value);
                if (item is null) return Enumerable.Empty<IEntity>();
                return new[] { item };
            }

            (bool IsIndex, string Name, object Value) IsIndex(ICriterion criterion)
            {
                var prop = EntityType.GetProperty(criterion.PropertyName);
                if (prop is null) return (false, null, null);

                var attr = prop.GetCustomAttribute<DynamoDBGlobalSecondaryIndexHashKeyAttribute>();
                if (attr?.GetType() != typeof(DynamoDBGlobalSecondaryIndexHashKeyAttribute)) return (false, null, null);

                return (true, attr.IndexNames.FirstOrDefault(), criterion.Value);
            }

            var indexInfo = query.Criteria.Select(IsIndex).ExceptNull().FirstOrDefault();
            if (indexInfo.IsIndex)
            {
                if (indexInfo.Value is null) return Enumerable.Empty<IEntity>();
                return (await Dynamo.Index<T>(indexInfo.Name).All(indexInfo.Value)).Cast<IEntity>();
            }

            ScanCondition ToCondition(ICriterion criterion)
            {
                ScanOperator GetOperator()
                {
                    switch (criterion.FilterFunction)
                    {
                        case FilterFunction.Is:
                            if (criterion.Value is null) return ScanOperator.IsNull;
                            return ScanOperator.Equal;
                        case FilterFunction.IsNot:
                            if (criterion.Value is null) return ScanOperator.IsNotNull;
                            return ScanOperator.NotEqual;
                        case FilterFunction.Null:
                            return ScanOperator.IsNull;
                        case FilterFunction.NotNull:
                            return ScanOperator.IsNotNull;
                        case FilterFunction.Contains:
                            return ScanOperator.Contains;
                        case FilterFunction.NotContains:
                            return ScanOperator.NotContains;
                        case FilterFunction.In:
                            return ScanOperator.In;
                        case FilterFunction.BeginsWith:
                            return ScanOperator.BeginsWith;
                        case FilterFunction.InRange:
                            return ScanOperator.Between;
                        case FilterFunction.LessThan:
                            return ScanOperator.LessThan;
                        case FilterFunction.LessThanOrEqual:
                            return ScanOperator.LessThanOrEqual;
                        case FilterFunction.MoreThan:
                            return ScanOperator.GreaterThan;
                        case FilterFunction.MoreThanOrEqual:
                            return ScanOperator.GreaterThanOrEqual;
                    }

                    throw new ArgumentOutOfRangeException(
                        nameof(criterion.FilterFunction),
                        $"{criterion.FilterFunction} isn't supported by DynamoDB."
                    );
                };

                var @operator = GetOperator();
                var values = new[] { criterion.Value }.ExceptNull().ToArray();

                return new ScanCondition(criterion.PropertyName, @operator, values);
            }

            return (await Dynamo.Db.ScanAsync<T>(query.Criteria.Select(ToCondition)).GetRemainingAsync()).Cast<IEntity>();
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