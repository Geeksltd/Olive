using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IDatabaseQuery
    {
        Type EntityType { get; }
        Dictionary<string, object> Parameters { get; }
        int PageStartIndex { get; set; }
        int? PageSize { get; set; }
        int? TakeTop { get; set; }
        string AliasPrefix { get; set; }
        string MapColumn(string propertyName);

        string Column(string propertyName, string alias = null);

        IDatabaseQuery Include(string associationProperty);
        IDatabaseQuery Include(IEnumerable<string> associationProperties);
        IDatabaseQuery Where(params ICriterion[] criteria);
        IDatabaseQuery WhereIn(string myField, IDatabaseQuery subquery, string targetField);
        IDatabaseQuery WhereNotIn(string myField, IDatabaseQuery subquery, string targetField);

        IDatabaseQuery OrderBy(string property, bool descending);
        IDatabaseQuery ThenBy(string property, bool descending);
        IDatabaseQuery Top(int rows);
        IDatabaseQuery OrderBy(string property);

        IDataProvider Provider { get; }
        List<ICriterion> Criteria { get; }
        List<OrderByPart> OrderByParts { get; }

        Task<IEntity> WithMax(string property);
        Task<IEntity> WithMin(string property);

        /// <summary>
        /// Transforms this query to be usable for a specified data provider. 
        /// </summary> 
        IDatabaseQuery CloneFor(Type entityType);

        Task<int> Count();
        Task<bool> Any();
        Task<bool> None();
        Task<IEnumerable<IEntity>> GetList();
        Task<IEntity> FirstOrDefault();
    }

    public interface IDatabaseQuery<TEntity> : IDatabaseQuery
        where TEntity : IEntity
    {
        IDatabaseQuery<TEntity> Where(Expression<Func<TEntity, bool>> criteria);
        new Task<IEnumerable<TEntity>> GetList();
        new Task<TEntity> FirstOrDefault();
        new IDatabaseQuery<TEntity> OrderBy(string property);
        new IDatabaseQuery<TEntity> Top(int rows);
        new IDatabaseQuery<TEntity> Where(params ICriterion[] criteria);

        IDatabaseQuery<TEntity> ThenBy(Expression<Func<TEntity, object>> property, bool descending = false);
        IDatabaseQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> property);
        IDatabaseQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> property, bool descending = false);
        IDatabaseQuery<TEntity> ThenByDescending(Expression<Func<TEntity, object>> property);
        IDatabaseQuery<TEntity> Include(Expression<Func<TEntity, object>> property);

        new Task<TEntity> WithMax(string property);
        new Task<TEntity> WithMin(string property);
        Task<TEntity> WithMax(Expression<Func<TEntity, object>> property);
        Task<TEntity> WithMin(Expression<Func<TEntity, object>> property);

        /// <summary>
        /// Gets a list of entities of the given type from the database with the specified type matching the specified criteria.
        /// If no criteria is specified, the count of all instances will be returned.
        /// </summary>        
        Task<TOutput?> Aggregate<TProperty, TOutput>(AggregateFunction function,
            Expression<Func<TEntity, TProperty?>> property)
            where TOutput : struct
            where TProperty : struct;

        Task<TProperty?> Max<TProperty>(Expression<Func<TEntity, TProperty?>> property) where TProperty : struct;
        Task<TProperty?> Max<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct;

        Task<TProperty?> Min<TProperty>(Expression<Func<TEntity, TProperty?>> property) where TProperty : struct;
        Task<TProperty?> Min<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct;

        Task<TProperty?> Sum<TProperty>(Expression<Func<TEntity, TProperty?>> property) where TProperty : struct;
        Task<TProperty?> Sum<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct;
        Task<decimal?> Sum(Expression<Func<TEntity, int?>> property);

        Task<TProperty?> Average<TProperty>(Expression<Func<TEntity, TProperty?>> property) where TProperty : struct;
        Task<TProperty?> Average<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct;
        Task<decimal?> Average(Expression<Func<TEntity, int?>> property);

        Task<bool> Contains(TEntity item);
        Task<bool> ContainsAny(TEntity[] items);
        Task<bool> Any(Expression<Func<TEntity, bool>> criteria);
    }
}