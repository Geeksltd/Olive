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
            @class = "validation",
            autocomplete = "off"
        };

        public IHtmlContent Generate<TModel, TProperty>(IHtmlHelper html, object viewModel, Expression<Func<TModel, TProperty>> property, object htmlAttributes)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var propertyInfo = property.GetProperty();
            var propertyName = propertyInfo.Name;
            var blob = propertyInfo.GetValue(viewModel) as BlobViewModel ?? new BlobViewModel();

            var result = new HtmlContentBuilder();

            string getId(string prop) => $"\"{propertyName}_{prop}\"";

            string GetHiddenInput(Expression<Func<BlobViewModel, object>> expression)
            {
                var propName = expression.GetProperty().Name;
                var id = getId(propName);
                var func = expression.Compile();
                return $@"<input type=""hidden"" id={id} name={id} class=""{propName}"" value=""{func(blob)}"" />";
            }

            result.AppendHtmlLine($@"
                <div class=""file-upload"">
                    <span class=""current-file"" aria-label=""Preview the file""{" style=\"display:none\"".OnlyWhen(blob.IsEmpty)}>
                        <a target=""_blank"" href=""{blob.Url?.HtmlEncode()}"">{blob.Filename.OrEmpty().HtmlEncode()}</a>
                    </span>
                    <label for={getId("fileInput")} hidden>HiddenLabel</label>
                    {html.TextBox(propertyName, "value".OnlyWhen(blob.HasValue), string.Empty, HiddenFieldSettings).GetString()}
                    <input type=""file"" id={getId("fileInput")} name=""files"" {OliveMvcExtensions.ToHtmlAttributes(htmlAttributes)}/>
                    {GetHiddenInput(x => x.Action)}
                    {GetHiddenInput(x => x.TempFileId)}
                    {GetHiddenInput(x => x.Filename)}
                    {GetHiddenInput(x => x.ItemId)}
                    {GetHiddenInput(x => x.Url)}
                    {GetHiddenInput(x => x.IsEmpty)}
                    <div class=""progress-bar"" role=""progressbar""></div>
                    <span class=""delete-file fa fa-remove btn"" style=""display: none""></span>
                </div>
            ");

            return result;
        }
    }
}
