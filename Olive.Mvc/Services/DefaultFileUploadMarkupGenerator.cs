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

        public IHtmlContent Generate<TModel, TProperty>(IHtmlHelper html, object viewModel, Expression<Func<TModel, TProperty>> property, object htmlAttributes)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var propertyInfo = property.GetProperty();
            var propertyName = propertyInfo.Name;
            var blob = propertyInfo.GetValue(viewModel) as Blob ?? Blob.Empty();

            var action = html.Request().HasFormContentType ? html.Request().Form[propertyName] : StringValues.Empty;
            if (action == "KEEP") blob = GetOldValue(viewModel, propertyName) ?? blob;

            var result = new HtmlContentBuilder();

            // For validation to work, this works instead of Hidden.
            if (action.ToString().IsEmpty() && blob.HasValue()) action = "KEEP";

            result.AppendHtmlLine($@"
                <div class=""file-upload"">
                    <span class=""current-file"" aria-label=""Preview the file""{" style=\"display:none\"".OnlyWhen(blob.IsEmpty())}>
                        <a target=""_blank"" href=""{blob.Url().HtmlEncode()}"">{blob.FileName.OrEmpty().HtmlEncode()}</a>
                    </span>
                    <label for=""{propertyName}_fileInput"" hidden>HiddenLabel</label>
                    <input type=""file"" id=""{propertyName}_fileInput"" name=""files"" {OliveMvcExtensions.ToHtmlAttributes(htmlAttributes)}/>
                    {html.TextBox(propertyName, action.OrEmpty(), string.Empty, HiddenFieldSettings).GetString()}
                    <div class=""progress-bar"" role=""progressbar""></div>
                    <span class=""delete-file fa fa-remove btn"" style=""display: none""></span>
                </div>
            ");

            return result;
        }

        Blob GetOldValue(object viewModel, string property)
        {
            var itemProperty = viewModel.GetType().GetProperty("Item") ??
                throw new Exception("Failed to find a property named 'Item' on this " + viewModel.GetType().GetProgrammingName());

            var item = itemProperty.GetValue(viewModel);
            if (item != null)
            {
                var originalPropertyInfo = item.GetType().GetProperty(property);
                if (originalPropertyInfo == null)
                {
                    Console.WriteLine($"Failed to find a property named '{property}' on " + item.GetType().GetProgrammingName());
                    return null;
                }

                return originalPropertyInfo.GetValue(item) as Blob ?? Blob.Empty();

                // Note: If this method is called with an IEnumerable<Blob> property, then the existing data will never be loaded.
            }

            return null;
        }
    }
}
