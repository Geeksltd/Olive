using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Mvc
{
    [ModelBinder(typeof(ListPaginationBinder))]
    public class ListPagination
    {
        public static string DefaultFirstText = "&laquo;";
        public static string DefaultPreviousText = "&lsaquo;";
        public static string DefaultNextText = "&rsaquo;";
        public static string DefaultLastText = "&raquo;";

        public static bool DisplayForSinglePage;
        public static string WrapperCssClass;
        public static bool DefaultShowFirstLastLinks = true;
        public static bool DefaultShowPreviousNextLinks;

        public bool ShowFirstLastLinks = DefaultShowFirstLastLinks;
        public bool ShowPreviousNextLinks = DefaultShowPreviousNextLinks;

        int? pageSize;
        bool IsPageSizeFromUrl, HasDefaultPageSize;

        public int CurrentPage { get; set; }

        /// <summary>
        /// The number of items on a page, or null for all of them.
        /// A size from the URL is only used if the list offers it: its default size, or one of its size options.
        /// Otherwise, as when the URL has no size, the default size is used.
        /// </summary>
        public int? PageSize
        {
            get => IsPageSizeFromUrl && !IsOffered(pageSize) ? DefaultPageSize : pageSize;
            set { pageSize = value; IsPageSizeFromUrl = false; }
        }

        /// <summary>
        /// The page size that the list was created with.
        /// </summary>
        public int? DefaultPageSize { get; private set; }
        public int TotalItemsCount { get; set; }
        public string Prefix { get; set; }
        public bool UseAjaxPost { get; set; }
        public bool UseAjaxGet { get; set; }

        public string FirstText { get; set; } = DefaultFirstText;
        public string PreviousText { get; set; } = DefaultPreviousText;
        public string NextText { get; set; } = DefaultNextText;
        public string LastText { get; set; } = DefaultLastText;

        public List<SelectListItem> SizeOptions = new List<SelectListItem>();
        public IViewModel Container;

        public int LastPage
        {
            get
            {
                if (PageSize == null) return 0;
                return (int)Math.Ceiling(1.0 * TotalItemsCount / PageSize.Value);
            }
        }

        public ListPagination(IViewModel container, int? pageSize = null)
        {
            CurrentPage = 1;
            PageSize = DefaultPageSize = pageSize;
            HasDefaultPageSize = true;
            Container = container;
        }

        /// <summary>
        /// Creates a pagination from a query string value such as "3-20" (page 3 of 20 items), with no default page size,
        /// so any page size in it is used.
        /// </summary>
        public ListPagination(IViewModel container, string queryInfo)
            : this(container)
        {
            HasDefaultPageSize = false;
            Parse(queryInfo);
        }

        /// <summary>
        /// Creates a pagination from a query string value such as "3-20" (page 3 of 20 items), for a list that was created as the default one.
        /// </summary>
        internal ListPagination(ListPagination @default, string queryInfo)
            : this(@default?.Container, @default?.DefaultPageSize)
        {
            HasDefaultPageSize = @default?.HasDefaultPageSize ?? false;
            Prefix = @default?.Prefix;
            UseAjaxPost = @default?.UseAjaxPost ?? false;
            UseAjaxGet = @default?.UseAjaxGet ?? false;
            Parse(queryInfo);
        }

        void Parse(string queryInfo)
        {
            if (queryInfo.IsEmpty()) return;

            // When a form has more than one page-size selector, the values are joined (e.g. "1-50|1-50").
            queryInfo = queryInfo.Split('|', ',').First().Trim();

            var parts = queryInfo.Split('-');
            CurrentPage = parts.First().TryParseAs<int>() ?? 1;
            if (CurrentPage < 1) CurrentPage = 1;

            pageSize = parts.ElementAtOrDefault(1).TryParseAs<int>();
            if (pageSize < 1) pageSize = null;
            IsPageSizeFromUrl = true;
        }

        bool IsOffered(int? size)
        {
            if (!HasDefaultPageSize || size == DefaultPageSize) return true;
            return SizeOptions.Any(x => GetSize(x) == size);
        }

        static int? GetSize(SelectListItem option) => option.Value.OrEmpty().Split('-').ElementAtOrDefault(1).TryParseAs<int>();

        public override string ToString() => CurrentPage + PageSize.ToStringOrEmpty().WithPrefix("-");

        public void Clear() => SizeOptions.Clear();

        public void AddPageSizeOptions(params object[] options)
        {
            foreach (var option in options)
            {
                var text = option.ToStringOrEmpty();

                var size = text.TryParseAs<int>();

                SizeOptions.Add(new SelectListItem
                {
                    Text = text,
                    Value = 1 + size.ToString().WithPrefix("-")
                });
            }

            // Only now are all the options known, which decide whether the page size from the URL is used.
            foreach (var option in SizeOptions) option.Selected = GetSize(option) == PageSize;
        }

        public string GetQuery(int pageNumber) => pageNumber + PageSize.ToStringOrEmpty().WithPrefix("-");

        public PagingQueryOption ToQueryOption(string orderBy = null)
        {
            orderBy = orderBy.Or("ID");

            var start = (CurrentPage - 1) * PageSize ?? 0;
            var size = PageSize ?? TotalItemsCount;

            return new PagingQueryOption(orderBy, start, size);
        }

        public PagingQueryOption ToQueryOption(ListSortExpression sort)
        {
            var orderBy = sort.Expression.Or("ID") + " DESC".OnlyWhen(sort.Descending);

            return ToQueryOption(orderBy);
        }
    }
}