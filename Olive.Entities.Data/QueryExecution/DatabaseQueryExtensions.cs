using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Olive.Entities
{
    public static partial class DatabaseQueryExtensions
    {
        public static T Page<T>(this T query, int startIndex, int pageSize)
            where T : IDatabaseQuery
        {
            if (pageSize < 1)
                throw new ArgumentException("Invalid PagingQueryOption specified. PageSize should be a positive number.");

            query.PageSize = pageSize;
            query.PageStartIndex = startIndex;
            return query;
        }

        public static T OrderBy<T>(this T query, string property, bool descending = false)
          where T : DatabaseQuery
        {
            return (T)((IDatabaseQuery)query).OrderBy(property, descending);
        }

        public static T OrderByDescending<T>(this T query, string property)
            where T : DatabaseQuery
        {
            return query.OrderBy(property, descending: true);
        }

        public static T ThenBy<T>(this T query, string property, bool descending = false)
            where T : DatabaseQuery
        {
            return (T)((IDatabaseQuery)query).ThenBy(property, descending);
        }

        public static T ThenByDescending<T>(this T query, string property)
            where T : DatabaseQuery
        {
            return query.ThenBy(property, descending: true);
        }

        public static T Where<T>(this T query, string sqlCriteria)
            where T : DatabaseQuery
        {
            query.Criteria.Add(new DirectDatabaseCriterion(sqlCriteria));
            return query;
        }

        public static T Where<T>(this T query, IEnumerable<ICriterion> criteria)
           where T : IDatabaseQuery
        {
            query.Where(criteria.ToArray());
            return query;
        }

        public static T Where<T>(this T query, IEnumerable<Criterion> criteria)
           where T : IDatabaseQuery
        {
            query.Where(criteria.Cast<ICriterion>().ToArray());
            return query;
        }

        #region WhereIn 

        public static T WhereIn<T>(this T query, string myField, IDatabaseQuery subquery, string targetField)
               where T : IDatabaseQuery
               => (T)query.WhereIn(myField, subquery, targetField);

        public static T WhereIn<T>(this T query, IDatabaseQuery subquery, string targetField)
               where T : IDatabaseQuery
               => query.WhereIn<T>("ID", subquery, targetField);

        public static T WhereIn<T>(this T query, Expression<Func<T, object>> property, IDatabaseQuery subquery, string targetField)
             where T : IDatabaseQuery
             => (T)query.WhereIn(property.GetPropertyPath(), subquery, targetField);

        public static T WhereIn<T, K>(this T query, Expression<Func<T, object>> property,
            K subquery, Expression<Func<K, object>> targetProperty)
             where T : IDatabaseQuery
            where K : IDatabaseQuery
             => (T)query.WhereIn(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());

        public static T WhereIn<T, K>(this T query, string property, K subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IDatabaseQuery
            => (T)query.WhereIn(property, subquery, targetProperty.GetPropertyPath());

        public static T WhereIn<T, K>(this T query, K subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IDatabaseQuery
           => query.WhereIn(subquery, targetProperty.GetPropertyPath());

        #endregion

        #region WhereNotIn
        public static T WhereNotIn<T>(this T query, string myField, IDatabaseQuery subquery, string targetField)
            where T : IDatabaseQuery
            => (T)query.WhereNotIn(myField, subquery, targetField);

        public static T WhereNotIn<T>(this T query, IDatabaseQuery subquery, string targetField)
               where T : IDatabaseQuery
               => query.WhereNotIn<T>("ID", subquery, targetField);

        public static T WhereNotIn<T>(this T query, Expression<Func<T, object>> property, IDatabaseQuery subquery, string targetField)
             where T : IDatabaseQuery
             => (T)query.WhereNotIn(property.GetPropertyPath(), subquery, targetField);

        public static T WhereNotIn<T, K>(this T query, Expression<Func<T, object>> property,
            K subquery, Expression<Func<K, object>> targetProperty)
             where T : IDatabaseQuery
            where K : IDatabaseQuery
             => (T)query.WhereNotIn(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());

        public static T WhereNotIn<T, K>(this T query, string property, K subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IDatabaseQuery
            => (T)query.WhereNotIn(property, subquery, targetProperty.GetPropertyPath());

        public static T WhereNotIn<T, K>(this T query, K subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IDatabaseQuery
           => query.WhereNotIn(subquery, targetProperty.GetPropertyPath());
        #endregion
    }
}