using System;
using System.Linq.Expressions;

namespace Olive.Entities
{
    namespace Data
    {
        partial class DatabaseQuery
        {
            public string MapColumn(string propertyName) => AliasPrefix + Provider.MapColumn(propertyName);

            public string AliasPrefix { get; set; }

            IDatabaseQuery IDatabaseQuery.WhereIn(string myField, IDatabaseQuery subquery, string targetField)
            {
                return WhereSubquery(myField, subquery, targetField, "IN");
            }

            IDatabaseQuery IDatabaseQuery.WhereNotIn(string myField, IDatabaseQuery subquery, string targetField)
            {
                return WhereSubquery(myField, subquery, targetField, "NOT IN");
            }

            IDatabaseQuery WhereSubquery(string myField, IDatabaseQuery subquery, string targetField, string @operator)
            {
                subquery.AliasPrefix = "Subq" + Guid.NewGuid().ToString().Remove("-").Substring(0, 6);

                var sql = subquery.Provider
                    .GenerateSelectCommand(subquery, subquery.MapColumn(targetField));

                sql = $"${{{{{myField}}}}} {@operator} ({sql})";
                Criteria.Add(Criterion.FromSql(sql, myField));

                foreach (var subQueryParam in subquery.Parameters)
                    Parameters.Add(subQueryParam.Key, subQueryParam.Value);

                return this;
            }
        }
    }

    partial class DatabaseQueryExtensions
    {
        #region WhereIn 

        public static T WhereIn<T>(this T query, string myField, IDatabaseQuery subquery, string targetField)
               where T : IDatabaseQuery
               => (T)query.WhereIn(myField, subquery, targetField);

        public static T WhereIn<T>(this T query, IDatabaseQuery subquery, string targetField)
               where T : IDatabaseQuery
               => query.WhereIn<T>("ID", subquery, targetField);

        public static IDatabaseQuery<T> WhereIn<T>(this IDatabaseQuery<T> query, Expression<Func<T, object>> property, IDatabaseQuery subquery, string targetField)
             where T : IEntity
             => (IDatabaseQuery<T>)query.WhereIn(property.GetPropertyPath(), subquery, targetField);

        public static IDatabaseQuery<T> WhereIn<T, K>(this IDatabaseQuery<T> query, Expression<Func<T, object>> property,
            IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
             where T : IEntity
            where K : IEntity
        {
            return (IDatabaseQuery<T>)query.WhereIn(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());
        }

        public static T WhereIn<T, K>(this T query, string property, IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IEntity
            => (T)query.WhereIn(property, subquery, targetProperty.GetPropertyPath());

        public static T WhereIn<T, K>(this T query, IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
            where T : IDatabaseQuery
            where K : IEntity
            => query.WhereIn(subquery, targetProperty.GetPropertyPath());

        #endregion

        #region WhereNotIn
        public static T WhereNotIn<T>(this T query, string myField, IDatabaseQuery subquery, string targetField)
            where T : IDatabaseQuery
        {
            return (T)query.WhereNotIn(myField, subquery, targetField);
        }

        public static T WhereNotIn<T>(this T query, IDatabaseQuery subquery, string targetField)
               where T : IDatabaseQuery
        {
            return query.WhereNotIn<T>("ID", subquery, targetField);
        }

        public static IDatabaseQuery<T> WhereNotIn<T>(this IDatabaseQuery<T> query, Expression<Func<T, object>> property, IDatabaseQuery subquery, string targetField)
             where T : IEntity
        {
            return (IDatabaseQuery<T>)query.WhereNotIn(property.GetPropertyPath(), subquery, targetField);
        }

        public static IDatabaseQuery<T> WhereNotIn<T, K>(this IDatabaseQuery<T> query, Expression<Func<T, object>> property,
            IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
             where T : IEntity
            where K : IEntity
        {
            return (IDatabaseQuery<T>)query.WhereNotIn(property.GetPropertyPath(),
                subquery, targetProperty.GetPropertyPath());
        }

        public static T WhereNotIn<T, K>(this T query, string property, IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IEntity
        {
            return (T)query.WhereNotIn(property, subquery, targetProperty.GetPropertyPath());
        }

        public static T WhereNotIn<T, K>(this T query, IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
           where T : IDatabaseQuery
           where K : IEntity
        {
            return query.WhereNotIn(subquery, targetProperty.GetPropertyPath());
        }
        #endregion
    }
}