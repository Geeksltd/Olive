using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class DatabaseQuery
    {
        /// <summary>
        /// Gets a list of entities of the given type from the database with the specified type matching the specified criteria.
        /// If no criteria is specified, the count of all instances will be returned.
        /// </summary>        
        public Task<object> Aggregate(AggregateFunction function, string propertyName)
        {
            return Provider.Aggregate(this, function, propertyName);
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        /// <summary>
        /// Gets a list of entities of the given type from the database with the specified type matching the specified criteria.
        /// If no criteria is specified, the count of all instances will be returned.
        /// </summary>        
        public async Task<TOutput?> Aggregate<TProperty, TOutput>(AggregateFunction function,
            Expression<Func<TEntity, TProperty>> property) where TOutput : struct
        {
            var result = await Aggregate(function, property.GetPropertyPath());
            return result.ToStringOrEmpty().TryParseAs<TOutput>();
        }

        public async Task<TProperty?> Max<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct =>
            await Aggregate<TProperty, TProperty>(AggregateFunction.Max, property);

        public async Task<TProperty?> Min<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct =>
            await Aggregate<TProperty, TProperty>(AggregateFunction.Min, property);

        public async Task<TProperty?> Sum<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct =>
            await Aggregate<TProperty, TProperty>(AggregateFunction.Sum, property);

        public async Task<TProperty?> Average<TProperty>(Expression<Func<TEntity, TProperty>> property)
            where TProperty : struct =>
            await Aggregate<TProperty, TProperty>(AggregateFunction.Average, property);

        public async Task<decimal?> Average<TProperty>(Expression<Func<TEntity, int>> property)
            where TProperty : struct =>
            await Aggregate<int, decimal>(AggregateFunction.Average, property);

        public async Task<decimal?> Average<TProperty>(Expression<Func<TEntity, int?>> property)
            where TProperty : struct =>
            await Aggregate<int?, decimal>(AggregateFunction.Average, property);

        public async Task<decimal?> Average(Expression<Func<TEntity, int>> property) =>
            await Aggregate<int, decimal>(AggregateFunction.Average, property);

        public async Task<decimal?> Average(Expression<Func<TEntity, int?>> property) =>
            await Aggregate<int?, decimal>(AggregateFunction.Average, property);
    }
}