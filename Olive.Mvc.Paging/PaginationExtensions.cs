using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Mvc
{
    public static class PaginationExtensions
    {
        const int DEFAULT_VISIBLE_PAGES = 7;
        const int DEFATLT_PAGE_SIZE = 100000;

        public static IEnumerable<T> TakePage<T>(this IEnumerable<T> list, ListPagination paging)
        {
            if (paging.PageSize == null) return list;

            var pageSize = paging.PageSize.Value;
            var currentPage = paging.CurrentPage.LimitMin(1);

            var skip = pageSize * (currentPage - 1);

            if (currentPage > 1 && skip > list.Count()) skip = 0;

            return list.Take(skip, pageSize);
        }

        public static T Page<T>(this T query, ListPagination paging)
            where T : IDatabaseQuery
        {
            query.Page((paging.CurrentPage - 1) * paging.PageSize ?? DEFATLT_PAGE_SIZE, paging.PageSize ?? DEFATLT_PAGE_SIZE);
            return query;
        }

        public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, object htmlAttributes = null, string prefix = null) =>
            Pagination(html, paging, DEFAULT_VISIBLE_PAGES, htmlAttributes, prefix);

        public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, int visiblePages, object htmlAttributes = null, string prefix = null) =>
            new PaginationRenderer(html, paging, visiblePages, htmlAttributes, prefix).Render();
    }
}