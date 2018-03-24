using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities;

namespace Olive.Mvc.Pagination
{
    public static class Extensions
    {
        const int DEFAULT_VISIBLE_PAGES = 7;

        public static IEnumerable<T> TakePage<T>(this IEnumerable<T> list, ListPagination paging) =>
            list.TakePage(paging.PageSize, paging.CurrentPage);

        public static T Page<T>(this T query, ListPagination paging)
            where T : IDatabaseQuery
        {
            query.Page(paging.CurrentPage, paging.PageSize ?? 100000);
            return query;
        }
        public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, object htmlAttributes = null, string prefix = null) =>
            Pagination(html, paging, DEFAULT_VISIBLE_PAGES, htmlAttributes, prefix);

        public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, int visiblePages, object htmlAttributes = null, string prefix = null) =>
            new PaginationRenderer(html, paging, visiblePages, htmlAttributes, prefix).Render();

    }
}
