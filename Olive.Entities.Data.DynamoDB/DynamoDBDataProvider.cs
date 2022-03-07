using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public partial class DynamoDBDataProvider<T> : IDataProvider
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

        public async Task BulkInsert(IEntity[] entities, int batchSize)
        {
            var writer = Dynamo.Db.CreateBatchWrite<T>();
            writer.AddPutItems(entities.Cast<T>());
            await writer.ExecuteAsync();

            SetIsNewAndOriginalId(entities);
        }

        public async Task BulkUpdate(IEntity[] entities, int batchSize)
        {
            var writer = Dynamo.Db.CreateBatchWrite<T>();
            entities.Do(x => writer.AddDeleteKey(x.GetId()));
            writer.AddPutItems(entities.Cast<T>());
            await writer.ExecuteAsync();

            SetIsNewAndOriginalId(entities);
        }

        public Task<int> Count(IDatabaseQuery query) => GetList(query).Count();

        public Task Delete(IEntity record) => Dynamo.Db.DeleteAsync((T)record);

        public Task<int> ExecuteNonQuery(string command) => throw new NotImplementedException();

        public Task<object> ExecuteScalar(string command) => throw new NotImplementedException();

        public string GenerateSelectCommand(IDatabaseQuery iquery, string fields)
        {
            throw new NotImplementedException();
        }

        public async Task<IEntity> Get(object objectID)
        {
            var result = (IEntity)await Dynamo.Table<T>().Get(objectID);
            return SetIsNewAndOriginalId(result);
        }

        public DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery masterQuery, PropertyInfo association)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            return await FindByHashKey(query) ?? await FindByIndex(query) ?? await FindByConditions(query);
        }

        async Task<IEnumerable<IEntity>> FindByHashKey(IDatabaseQuery query)
        {
            if (query.Criteria.HasMany()) return null;

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

                SetIsNewAndOriginalId(item);
                return new[] { item };
            }

            return null;
        }

        async Task<IEnumerable<IEntity>> FindByIndex(IDatabaseQuery query)
        {
            if (query.Criteria.HasMany()) return null;

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

                var result = (await Dynamo.Index<T>(indexInfo.Name).All(indexInfo.Value)).Cast<IEntity>();
                return SetIsNewAndOriginalId(result);
            }

            return null;
        }

        async Task<IEnumerable<IEntity>> FindByConditions(IDatabaseQuery query)
        {
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

            var result = (await Dynamo.Table<T>().All(query.Criteria.Select(ToCondition).ToArray())).Cast<IEntity>();
            return SetIsNewAndOriginalId(result);
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

        public async Task Save(IEntity record)
        {
            var entityType = record.GetType();

            var tableName = entityType.GetCustomAttribute<DynamoDBTableAttribute>().TableName;
            var properties = entityType.GetProperties()
                .Select(x => UpdatePropertyData.FromProperty(x, record))
                .ToArray();
            var hashKeyProp = properties.Single(x => x.IsHashKey);
            var propertiesToUpdate = properties.Except(hashKeyProp).ToArray();

            var key = new Dictionary<string, AttributeValue> { {
                hashKeyProp.Name, hashKeyProp.GetAttributeValue()
            } };
            var updates = propertiesToUpdate.ToDictionary(
                x => x.Name, x => x.GetAttributeValueUpdate()
            );

            await Dynamo.Client.UpdateItemAsync(
                new UpdateItemRequest(tableName, key, updates)
            );

            SetIsNewAndOriginalId(record);
        }

        public bool SupportValidationBypassing() => throw new NotImplementedException();

        IEntity SetIsNewAndOriginalId(IEntity entity)
        {
            Entity.Services.SetSaved(entity);
            Entity.Services.SetOriginalId(entity);
            return entity;
        }
        IEnumerable<IEntity> SetIsNewAndOriginalId(IEnumerable<IEntity> entities)
        {
            entities.Do(i => SetIsNewAndOriginalId(i));
            return entities;
        }
    }
}