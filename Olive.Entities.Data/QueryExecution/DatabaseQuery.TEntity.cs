namespace Olive.Entities.Data
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

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

        public Task<bool> Contains(TEntity item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return ContainsAny(item);
        }

        public Task<bool> ContainsAny(params TEntity[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var ids = items.Where(x => !ReferenceEquals(null, x)).Select(x => x.GetId()).ToArray();

            if (ids.None()) throw new ArgumentException("ContainsAny() requires at least one item");

            if (ids[0] is string || ids[0] is Guid)
                ids = ids.Select(x => $"'{x}'").ToArray();

            var idColumn = Provider.MapColumn("ID");

            ((IDatabaseQuery<TEntity>)this)
                .Where(new DirectDatabaseCriterion(ids.Select(x => $"{idColumn} = {x}").ToString(" OR ")));

            return Any();
        }

        public Task<bool> Any(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria != null)
                ((IDatabaseQuery<TEntity>)this).Where(criteria);
            return Any();
        }
    }
}