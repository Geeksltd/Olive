using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Olive.Entities;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive.Mvc
{
    public class DefaultFileUploadMarkupGenerator : IFileUploadMarkupGenerator
    {
        readonly object LockObject = new object();
        readonly object HiddenFieldSettings = new
        {
            tabindex = "-1",
            style = "width:1px; height:0; border:0; padding:0; margin:0;",
            @class = "validation",
            autocomplete = "off"
        };

        PropertyInfo PropertyInfo;
        BlobViewModel Blob;

        public IHtmlContent Generate<TModel, TProperty>(IHtmlHelper html, object viewModel, Expression<Func<TModel, TProperty>> property, object htmlAttributes)
        {
            lock(LockObject)
                return DoGenerate(html, viewModel, property, htmlAttributes);
        }

        protected virtual IHtmlContent DoGenerate<TModel, TProperty>(IHtmlHelper html, object viewModel, Expression<Func<TModel, TProperty>> property, object htmlAttributes)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (property == null) throw new ArgumentNullException(nameof(property));

            PropertyInfo = property.GetProperty();
            Blob = PropertyInfo.GetValue(viewModel) as BlobViewModel ?? new BlobViewModel();

            var result = new HtmlContentBuilder();

            result.AppendHtmlLine($@"
                <div class=""file-upload"">
                    <span class=""current-file"" aria-label=""Preview the file""{" style=\"display:none\"".OnlyWhen(Blob.IsEmpty)}>
                        <a target=""_blank"" href=""{Blob.Url?.HtmlEncode()}"">{Blob.Filename.OrEmpty().HtmlEncode()}</a>
                    </span>
                    <label for={GetId("fileInput")} hidden>HiddenLabel</label>
                    {html.TextBox(PropertyInfo.Name, "value".OnlyWhen(Blob.HasValue), string.Empty, HiddenFieldSettings).GetString()}
                    <input type=""file"" id={GetId("fileInput")} name=""files"" {GetHtmlAttributes(htmlAttributes)}/>
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

        protected virtual string GetHtmlAttributes(object htmlAttributes) =>
            OliveMvcExtensions.ToHtmlAttributes(htmlAttributes);

        string GetId(string prop) => $"\"{PropertyInfo.Name}_{prop}\"";

        string GetHiddenInput(Expression<Func<BlobViewModel, object>> expression)
        {
            var propName = expression.GetProperty().Name;
            var id = GetId(propName);
            var func = expression.Compile();
            return $@"<input type=""hidden"" id={id} name={id} class=""{propName}"" value=""{func(Blob)}"" />";
        }
    }
}
