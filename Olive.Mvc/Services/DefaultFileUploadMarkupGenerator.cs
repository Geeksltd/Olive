using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Olive.Entities;
using System;
using System.Linq.Expressions;

namespace Olive.Mvc
{
    class DefaultFileUploadMarkupGenerator
    {
        static object HiddenFieldSettings = new
        {
            tabindex = "-1",
            style = "width:1px; height:0; border:0; padding:0; margin:0;",
            @class = "file-id",
            autocomplete = "off"
        };

        public IHtmlContent Generate<TModel, TProperty>(IHtmlHelper html, object model, Expression<Func<TModel, TProperty>> property, object htmlAttributes)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var propertyInfo = property.GetProperty();
            var propertyName = propertyInfo.Name;
            var blob = propertyInfo.GetValue(model) as Blob ?? Blob.Empty();

            var action = html.Request().HasFormContentType ? html.Request().Form[propertyName] : StringValues.Empty;
            if (action == "KEEP") blob = GetOldValue(model, propertyName) ?? blob;

            var result = new HtmlContentBuilder();
            result.AppendHtmlLine("<div class=\"file-upload\">");
            result.AppendHtmlLine($"<span class=\"current-file\" aria-label=\"Preview the file\"{" style=\"display:none\"".OnlyWhen(blob.IsEmpty())}>");
            result.AppendHtmlLine($"<a target=\"_blank\" href=\"{blob.Url().HtmlEncode()}\">{blob.FileName.OrEmpty().HtmlEncode()}</a>");
            result.AppendHtmlLine("</span>");

            result.AppendHtmlLine($"<label for=\"{propertyName}_fileInput\" hidden>HiddenLabel</label>");
            result.AppendHtmlLine($"<input type=\"file\" id=\"{propertyName}_fileInput\" name=\"files\" {OliveMvcExtensions.ToHtmlAttributes(htmlAttributes)}/>");

            // For validation to work, this works instead of Hidden.
            if (action.ToString().IsEmpty() && blob.HasValue()) action = "KEEP";
            result.AppendHtml(html.TextBox(propertyName, action.OrEmpty(), string.Empty, HiddenFieldSettings));
            result.AppendHtmlLine("<div class=\"progress-bar\" role=\"progressbar\"></div>");
            result.AppendHtmlLine("<span class=\"delete-file fa fa-remove btn\" style=\"display: none\"></span>");
            result.AppendHtmlLine("</div>");

            return result;
        }

        Blob GetOldValue(object model, string property)
        {
            var itemProperty = model.GetType().GetProperty("Item");
            if (itemProperty == null)
                throw new Exception("Failed to find a property named 'Item' on this " + model.GetType().GetProgrammingName());

            var item = itemProperty.GetValue(model);
            if (item != null)
            {
                var originalPropertyInfo = item.GetType().GetProperty(property);
                if (originalPropertyInfo == null)
                    throw new Exception($"Failed to find a property named '{property }' on " + item.GetType().GetProgrammingName());

                return originalPropertyInfo.GetValue(item) as Blob ?? Blob.Empty();

                // Note: If this method is called with an IEnumerable<Blob> property, then the existing data will never be loaded.
            }

            return null;
        }
    }
}
