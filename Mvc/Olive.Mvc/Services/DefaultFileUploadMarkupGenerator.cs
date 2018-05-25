using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities;

namespace Olive.Mvc
{
    class DefaultFileUploadMarkupGenerator
    {
        public IHtmlContent Generate<TModel, TProperty>(IHtmlHelper html, object model, Expression<Func<TModel, TProperty>> property, object htmlAttributes)
        {
            var propertyInfo = property.GetProperty();
            var blob = propertyInfo.GetValue(model) as Blob ?? Blob.Empty();
            var value = html.ViewContext.HttpContext.Request.HasFormContentType ?
                html.ViewContext.HttpContext.Request.Form[propertyInfo.Name] :
                Microsoft.Extensions.Primitives.StringValues.Empty;
            if (value == "KEEP")
            {
                var itemProperty = model.GetType().GetProperty("Item");
                var item = itemProperty.GetValue(model);
                var originalPropertyInfo = item.GetType().GetProperty(propertyInfo.Name);
                blob = originalPropertyInfo.GetValue(item) as Blob ?? Blob.Empty();
            }

            // Note: If this method is called with an IEnumerable<Blob> property,
            // then the existing data will never be loaded.
            var result = new HtmlContentBuilder();

            result.AppendHtmlLine("<div class=\"file-upload\">");
            result.AppendHtmlLine($"<span class=\"current-file\"{" style=\"display:none\"".OnlyWhen(blob.IsEmpty())}>" +
                $"<a target=\"_blank\" href=\"{blob.Url().HtmlEncode()}\">{blob.FileName.OrEmpty().HtmlEncode()}</a></span>");

            result.AppendHtmlLine($"<label for=\"{propertyInfo.Name}_fileInput\" hidden>HiddenLabel</label>");
            result.AppendHtmlLine($"<input type=\"file\" id=\"{propertyInfo.Name}_fileInput\" name=\"files\" {OliveMvcExtensions.ToHtmlAttributes(htmlAttributes)}/>");

            // For validation to work, this works instead of Hidden.
            if (value.ToString().IsEmpty() && blob.HasValue()) value = "KEEP";
            result.AppendHtml(html.TextBox(propertyInfo.Name, value.OrEmpty(), string.Empty,
                new { tabindex = "-1", style = "width:1px; height:0; border:0; padding:0; margin:0;", @class = "file-id", autocomplete = "off" }));
            result.AppendHtmlLine("<div class=\"progress-bar\" role=\"progressbar\"></div>");
            result.AppendHtmlLine("<span class=\"delete-file fa fa-remove btn\" style=\"display: none\"></span>");
            result.AppendHtmlLine("</div>");

            return result;
        }
    }
}
