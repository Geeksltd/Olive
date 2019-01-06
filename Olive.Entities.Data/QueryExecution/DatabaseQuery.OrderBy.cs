using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Olive.Entities.Data
{
    partial class DatabaseQuery
    {
        internal const bool DESC = true;
        public List<OrderByPart> OrderByParts { get; } = new List<OrderByPart>();

        IDatabaseQuery IDatabaseQuery.OrderBy(string property, bool descending)
        {
            if (OrderByParts.Any())
                throw new Exception("There is already an OrderBy part added. Use ThenBy().");

            return this.ThenBy(property, descending);
        }

        IDatabaseQuery IDatabaseQuery.ThenBy(string property, bool descending)
        {
            OrderByParts.Add(new OrderByPart { Property = property, Descending = descending });
            return this;
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        public IDatabaseQuery<TEntity> ThenBy(Expression<Func<TEntity, object>> property, bool descending = false)
        {
            var propertyPath = property.Body.GetPropertyPath();
            if (propertyPath.IsEmpty() || propertyPath.Contains("."))
                throw new Exception($"Unsupported OrderBy expression. The only supported format is \"x => x.Property\". You provided: {property}");

            return this.ThenBy(propertyPath, descending);
        }

        public IDatabaseQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> property) => OrderBy(property, DESC);

        public IDatabaseQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> property, bool descending = false)
        {
            if (OrderByParts.Any())
                throw new Exception("There is already an OrderBy part added. Use ThenBy().");

            return ThenBy(property, descending);
        }

        public IDatabaseQuery<TEntity> ThenByDescending(Expression<Func<TEntity, object>> property) => ThenBy(property, DESC);
    }
}