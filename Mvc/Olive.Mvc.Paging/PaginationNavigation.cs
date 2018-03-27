namespace Olive.Mvc
{
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.Rendering;

    class PaginationRenderer
    {
        IHtmlHelper Html;
        ListPagination Paging;
        int VisiblePages, Start, End;
        object HtmlAttributes;
        string Prefix;

        public PaginationRenderer(IHtmlHelper html, ListPagination paging, int visiblePages, object htmlAttributes, string prefix)
        {
            Html = html;
            Paging = paging;
            VisiblePages = visiblePages;
            HtmlAttributes = htmlAttributes;
            Prefix = prefix;
        }

        void FindBoundaries()
        {
            if (Paging.CurrentPage > Paging.LastPage) Paging.CurrentPage = 1;

            Start = Paging.CurrentPage;
            End = Paging.CurrentPage;

            while ((Start > 1 || End < Paging.LastPage) && End - Start < VisiblePages - 1)
            {
                if (Start > 1) Start--;
                if (End - Start < VisiblePages - 1 && End < Paging.LastPage) End++;
            }
        }

        string GetPagingKey() => Paging.Prefix.WithSuffix(".") + "p";

        string GetLinkAttributes(int number)
        {
            var urlHelper = Context.Current.Http().GetUrlHelper();

            if (Paging.UseAjaxPost)
            {
                return "href=\"#\" formaction=\"{0}\" data-pagination=\"{1}{2}\""
                    .FormatWith(urlHelper.ActionWithQuery(Paging.Container.GetType().Name + "/Reload"), Paging.Prefix.WithSuffix(".p="), Paging.GetQuery(number));
            }
            else
            {
                var url = urlHelper.Current(new Dictionary<string, string> { { GetPagingKey(), Paging.GetQuery(number) } });

                return "href=\"{0}\"".FormatWith(url) + " data-redirect=\"ajax\"".OnlyWhen(Paging.UseAjaxGet);
            }
        }

        public HtmlString Render()
        {
            if (Paging.PageSize == null || Paging.TotalItemsCount == 0)
                return null;

            if (ListPagination.DisplayForSinglePage == false && Paging.LastPage == 1)
                return null;

            FindBoundaries();

            var r = new StringBuilder();

            if (ListPagination.WrapperCssClass.HasValue())
                r.AppendLine($"<div class=\"{ListPagination.WrapperCssClass}\">");

            r.AddFormattedLine("<ul{0}>", OliveMvcExtensions.ToHtmlAttributes(HtmlAttributes));

            var isFirst = Paging.CurrentPage == 1;
            var isLast = Paging.CurrentPage == Paging.LastPage;

            // add first page control
            if (Paging.ShowFirstLastLinks)
                AddPaginationControl(r, Paging.FirstText, "First page", Paging.CurrentPage == 1, 1);

            // add previous page control
            if (Paging.ShowPreviousNextLinks)
            {
                var previousPage = isFirst ? 1 : Paging.CurrentPage - 1;
                AddPaginationControl(r, Paging.PreviousText, "Previous page", isFirst, previousPage);
            }

            for (var i = Start; i <= End; i++)
            {
                r.AppendFormat("<li {0}>", " class=\"active\"".OnlyWhen(i == Paging.CurrentPage));
                r.AddFormattedLine("<a {0}>{1}</a></li>", GetLinkAttributes(i), i);
            }

            // add next page control
            if (Paging.ShowPreviousNextLinks)
            {
                var nextPage = isLast ? Paging.LastPage : Paging.CurrentPage + 1;
                AddPaginationControl(r, Paging.NextText, "Next page", isLast, nextPage);
            }

            // add last page control
            if (Paging.ShowFirstLastLinks)
                AddPaginationControl(r, Paging.LastText, "Last page", isLast, Paging.LastPage);

            r.AppendLine("</ul>");
            r.AppendLineIf("</div>", ListPagination.WrapperCssClass.HasValue());

            return new HtmlString(r.ToString());
        }

        void AddPaginationControl(StringBuilder builder, string text, string ariaText, bool isDisabled, int pageNumber)
        {
            builder.AppendFormat("<li{0}>", " class=\"disabled\"".OnlyWhen(isDisabled));
            builder.AddFormattedLine("<a {0} aria-label=\"{1}\">", GetLinkAttributes(pageNumber), ariaText);
            builder.AddFormattedLine("<span aria-hidden=\"true\">{0}</span>", text);
            builder.AppendLine("</a>");
            builder.AppendLine("</li>");
        }
    }

}