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
            => Provider.Aggregate(this, function, propertyName);

        protected Task<TOutput?> Aggregate<TOutput>(AggregateFunction function, Expression property)
            where TOutput : struct
            => Aggregate(function, property.GetPropertyPath()).Get(x => x.ToStringOrEmpty().TryParseAs<TOutput>());
    }

    partial class DatabaseQuery<TEntity>
    {
        public Task<TOutput?> Aggregate<TProperty, TOutput>(AggregateFunction function,
             Expression<Func<TEntity, TProperty?>> property)
             where TOutput : struct
             where TProperty : struct
            => base.Aggregate<TOutput>(function, property);

        public Task<TProperty?> Max<TProperty>(Expression<Func<TEntity, TProperty?>> property)
            where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Max, property);

        public Task<TProperty?> Max<TProperty>(Expression<Func<TEntity, TProperty>> property)
            where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Max, property);

        public Task<TProperty?> Min<TProperty>(Expression<Func<TEntity, TProperty?>> property)
            where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Min, property);

        public Task<TProperty?> Min<TProperty>(Expression<Func<TEntity, TProperty>> property)
            where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Min, property);

        public Task<TProperty?> Sum<TProperty>(Expression<Func<TEntity, TProperty?>> property)
            where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Sum, property);

        public Task<TProperty?> Sum<TProperty>(Expression<Func<TEntity, TProperty>> property)
            where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Sum, property);

        public Task<decimal?> Sum(Expression<Func<TEntity, int?>> property)
            => Aggregate<decimal>(AggregateFunction.Average, property);

        public Task<TProperty?> Average<TProperty>(Expression<Func<TEntity, TProperty?>> property)
          where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Average, property);

        public Task<TProperty?> Average<TProperty>(Expression<Func<TEntity, TProperty>> property)
          where TProperty : struct
            => Aggregate<TProperty>(AggregateFunction.Average, property);

        public Task<decimal?> Average(Expression<Func<TEntity, int?>> property)
            => Aggregate<decimal>(AggregateFunction.Average, property);
    }
}