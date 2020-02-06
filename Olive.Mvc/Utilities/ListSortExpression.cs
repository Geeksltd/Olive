using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class ListSortExpression
    {
        public string Expression { get; set; }
        public bool Descending { get; set; }
        public string Prefix { get; set; }
        public bool UseAjaxPost { get; set; }
        public IViewModel Container { get; set; }

        public ListSortExpression(IViewModel container) { Container = container; }

        public ListSortExpression(IViewModel container, string expression) : this(container)
        {
            Descending = expression?.EndsWith(".DESC") ?? false;
            Expression = expression?.TrimEnd(".DESC");
        }

        public IEnumerable<TSource> Apply<TSource, TSort>(IEnumerable<TSource> items, Func<TSource, TSort> expression)
        {
            if (Descending) return items.OrderByDescending(expression);
            else return items.OrderBy(expression);
        }

        public async Task<IEnumerable<TSource>> Apply<TSource, TSort>(IEnumerable<TSource> items, Func<TSource, Task<TSort>> expression)
        {
            if (Descending) return await items.OrderByDescending(expression);
            else return await items.OrderBy(expression);
        }

        public IEnumerable<T> Apply<T>(IEnumerable<T> items)
        {
            if (items == null) return Enumerable.Empty<T>();

            if (Expression.IsEmpty()) return items;

            try
            {
                if (Descending) return items.OrderByDescending(Expression);
                else return items.OrderBy(Expression);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not sort the " + typeof(T).Name.ToPlural() + " list with expression: " + Expression, ex);
            }
        }

        public override string ToString() => Expression + ".DESC".OnlyWhen(Descending);

        public string Url(string sortExpression)
        {
            sortExpression = sortExpression.OrEmpty();

            if (UseAjaxPost)
                return Context.Current.Http().GetUrlHelper().ActionWithQuery(Container.GetType().Name + "/Reload");
            else
                return UrlForGet(sortExpression);
        }

        string UrlForGet(string sortExpression)
        {
            var result = Context.Current.Http().GetUrlHelper().CurrentUri();

            var queryKey = Prefix.WithSuffix(".").ToLower() + "s";

            var currentSort = result.GetQueryString().TryGet(queryKey).Or(Expression);

            if (currentSort.HasValue())
            {
                if (currentSort == sortExpression) sortExpression += ".DESC";
                else if (currentSort == sortExpression + ".DESC") sortExpression = sortExpression.TrimEnd(".DESC");
            }

            result = result.RemoveQueryString(queryKey).AddQueryString(queryKey, sortExpression);

            return result.PathAndQuery;
        }
    }
}