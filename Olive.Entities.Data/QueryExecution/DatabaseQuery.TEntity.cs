namespace Olive.Entities.Data
{
    using System;
    using System.Linq.Expressions;

    public partial class DatabaseQuery<TEntity> : DatabaseQuery, IDatabaseQuery<TEntity>
    where TEntity : IEntity
    {
        public DatabaseQuery() : base(typeof(TEntity)) { }

        IDatabaseQuery<TEntity> IDatabaseQuery<TEntity>.Where(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null) return this;
            Criteria.AddRange(CriteriaExtractor<TEntity>.Parse(criteria));
            return this;
        }

        IDatabaseQuery<TEntity> IDatabaseQuery<TEntity>.Include(Expression<Func<TEntity, object>> property)
        {
            ((IDatabaseQuery)this).Include(property.GetPropertyPath());
            return this;
        }

        IDatabaseQuery<TEntity> IDatabaseQuery<TEntity>.Top(int rows)
        {
            TakeTop = rows;
            return this;
        }

        IDatabaseQuery<TEntity> IDatabaseQuery<TEntity>.OrderBy(string property)
            => this.OrderBy(property, descending: false);

        IDatabaseQuery<TEntity> IDatabaseQuery<TEntity>.Where(params ICriterion[] criteria)
        {
            Criteria.AddRange(criteria);
            return this;
        }
    }
}