using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Olive.Entities
{
    /// <summary>
    /// A data provider implementation that throws a not supported exception for all methods.
    /// </summary>
    public abstract class LimitedDataProvider : IDataProvider
    {
        Exception NA([CallerMemberName] string method = null)
            => new NotSupportedException($"IDataProvider.{method}() is not supported in {GetType().Name}.");

        public virtual IDataAccess Access => throw NA();

        public virtual string ConnectionString { get; set; }
        public virtual string ConnectionStringKey { get; set; }

        public virtual Type EntityType => throw NA();

        public virtual Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName) => throw NA();

        public virtual Task BulkInsert(IEntity[] entities, int batchSize) => throw NA();

        public virtual Task BulkUpdate(IEntity[] entities, int batchSize) => throw NA();

        public virtual Task<int> Count(IDatabaseQuery query) => throw NA();

        public virtual Task Delete(IEntity record) => throw NA();

        public virtual Task<int> ExecuteNonQuery(string command) => throw NA();

        public virtual Task<object> ExecuteScalar(string command) => throw NA();

        public virtual Task<IEntity> Get(object objectID) => throw NA();

        public virtual DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery query, PropertyInfo association) => throw NA();

        public virtual Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query) => throw NA();

        public virtual IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated) => throw NA();

        public virtual string MapColumn(string propertyName) => throw NA();

        public virtual string MapSubquery(string path, string parent) => throw NA();

        public virtual Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property) => throw NA();

        public virtual Task Save(IEntity record) => throw NA();

        public virtual bool SupportValidationBypassing() => throw NA();
    }
}