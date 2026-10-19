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

            /// <summary>[NOT] EXISTS (the subquery, where its targetField equals this query's myField): a
            /// correlated subquery, which unlike NOT IN is not emptied by a NULL in the subquery's column.</summary>
            internal DatabaseQuery WhereCorrelated(string myField, IDatabaseQuery subquery, string targetField, string @operator)
            {
                // A token rather than the placeholder itself: the subquery would map the placeholder against its
                // own entity. It becomes this query's placeholder once the subquery's SQL is generated.
                const string OuterColumn = "__OliveOuterColumn__";

                subquery.AliasPrefix = "Subq" + Guid.NewGuid().ToString().Remove("-").Substring(0, 6);
                subquery.Where(Criterion.FromSql($"{subquery.MapColumn(targetField)} = {OuterColumn}"));

                var sql = subquery.Provider.GenerateSelectCommand(subquery, "1")
                    .Replace(OuterColumn, $"${{{{{myField}}}}}");

                Criteria.Add(Criterion.FromSql($"{@operator} ({sql})", myField));

                foreach (var subQueryParam in subquery.Parameters)
                    Parameters[subQueryParam.Key] = subQueryParam.Value;

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

        #region WhereExists

        /// <summary>Keeps the rows for which the subquery has a row whose <paramref name="targetField"/> equals
        /// this row's <paramref name="myField"/> (a correlated EXISTS).</summary>
        public static T WhereExists<T>(this T query, string myField, IDatabaseQuery subquery, string targetField)
            where T : IDatabaseQuery
        {
            ToDatabaseQuery(query).WhereCorrelated(myField, subquery, targetField, "EXISTS");
            return query;
        }

        /// <summary>Keeps the rows for which the subquery has no row whose <paramref name="targetField"/> equals
        /// this row's <paramref name="myField"/> (a correlated NOT EXISTS). Unlike WhereNotIn, a NULL in the
        /// subquery's column does not empty the result.</summary>
        public static T WhereNotExists<T>(this T query, string myField, IDatabaseQuery subquery, string targetField)
            where T : IDatabaseQuery
        {
            ToDatabaseQuery(query).WhereCorrelated(myField, subquery, targetField, "NOT EXISTS");
            return query;
        }

        public static IDatabaseQuery<T> WhereExists<T, K>(this IDatabaseQuery<T> query, Expression<Func<T, object>> property,
            IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
            where T : IEntity
            where K : IEntity
            => query.WhereExists(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());

        public static IDatabaseQuery<T> WhereNotExists<T, K>(this IDatabaseQuery<T> query, Expression<Func<T, object>> property,
            IDatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
            where T : IEntity
            where K : IEntity
            => query.WhereNotExists(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());

        static Data.DatabaseQuery ToDatabaseQuery(IDatabaseQuery query) =>
            query as Data.DatabaseQuery ?? throw new NotSupportedException($"{query?.GetType().Name} does not support WhereExists.");

        #endregion
    }
}