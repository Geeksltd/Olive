using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities;

namespace Olive.Mvc
{
    public class ListPagination
    {
        public static string DefaultFirstText = "&laquo;";
        public static string DefaultPreviousText = "&lsaquo;";
        public static string DefaultNextText = "&rsaquo;";
        public static string DefaultLastText = "&raquo;";

        public static bool DisplayForSinglePage = false;
        public static string WrapperCssClass;
        public static bool DefaultShowFirstLastLinks = true;
        public static bool DefaultShowPreviousNextLinks = false;

        public bool ShowFirstLastLinks = DefaultShowFirstLastLinks;
        public bool ShowPreviousNextLinks = DefaultShowPreviousNextLinks;

        public int CurrentPage { get; set; }
        public int? PageSize { get; set; }
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
            PageSize = pageSize;
            Container = container;
        }

        public ListPagination(IViewModel container, string queryInfo)
            : this(container)
        {
            if (queryInfo.IsEmpty()) return;

            var parts = queryInfo.Split('-');
            CurrentPage = parts.First().TryParseAs<int>() ?? 1;
            if (CurrentPage < 1) CurrentPage = 1;

            if (parts.HasMany())
            {
                PageSize = parts.ElementAt(1).TryParseAs<int>();
                if (PageSize < 1) PageSize = null;
            }
        }

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
                    Value = 1 + size.ToString().WithPrefix("-"),
                    Selected = size == PageSize
                });
            }
        }

        public string GetQuery(int pageNumber) => pageNumber + PageSize.ToStringOrEmpty().WithPrefix("-");

        public PagingQueryOption ToQueryOption(string orderBy = null)
        {
            orderBy = orderBy.Or("ID");

            var start = CurrentPage * PageSize ?? 0;
            var size = PageSize ?? TotalItemsCount;

            return new PagingQueryOption(orderBy, start, size);
        }

        public PagingQueryOption ToQueryOption(ListSortExpression sort)
        {
            var orderBy = sort.Expression.Or("ID") + " DESC".OnlyWhen(sort.Descending);

            return ToQueryOption(orderBy);
        }
    }

    public class ColumnSelectionBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null) return Task.CompletedTask; ;

            var result = bindingContext.Model as ColumnSelection;

            result?.SetSelection(value.FirstValue.OrEmpty().Split('|'));

            return Task.CompletedTask;
        }
    }
}