using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}