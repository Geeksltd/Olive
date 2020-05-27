using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Olive;
using Olive.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        public static HtmlString ToJson(this IHtmlHelper @this, object obj)
        {
            if (obj == null) return new HtmlString("[]");

            return new HtmlString(JsonConvert.SerializeObject(obj));
        }

        public static HtmlString GetActionsJson(this IHtmlHelper @this)
        {
            var data = @this.ViewContext.HttpContext.JavascriptActions();
            return @this.ToJson(data);
        }

        public static IHtmlContent RadioButtonsFor<TModel, TProperty>(this IHtmlHelper<TModel> @this, Expression<Func<TModel, TProperty>> property, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            var propertyInfo = property.GetProperty();

            var value = propertyInfo.GetValue(@this.ViewData.Model);

            if (value is IEntity) value = (value as IEntity).GetId();

            var settings = ToHtmlAttributes(htmlAttributes);

            var result = new HtmlContentBuilder();

            result.AppendHtmlLine($"<div class=\"radio-list\" id=\"{propertyInfo.Name}\">");

            foreach (var item in selectList)
            {
                result.AppendHtmlLine($"<div{settings}>");

                var id = propertyInfo.Name + "_" + selectList.IndexOf(item);

                result.AppendHtml(@this.RadioButton(propertyInfo.Name, item.Value, IsSelected(item, value),
                    new { id, @class = "form-check-input" }));

                result.AppendHtmlLine($"<label for=\"{id}\" class=\"form-check-label\">{item.Text}</label>");

                result.AppendHtmlLine("</div>");
            }

            result.AppendHtmlLine("</div>");

            return result;
        }

        static bool IsSelected(SelectListItem item, object boundValue)
        {
            if (boundValue.ToStringOrEmpty() == item.Value) return true;

            if (boundValue == null && item.Value == "-") return true;

            return false;
        }

        public static IHtmlContent FileUploadFor<TModel>(this IHtmlHelper<TModel> @this, Expression<Func<TModel, IEnumerable<BlobViewModel>>> property, object htmlAttributes = null)
        {
            return Context.Current.GetService<IFileUploadMarkupGenerator>()
                .Generate(@this, @this.ViewData.Model, property, htmlAttributes);
        }

        public static IHtmlContent FileUploadFor<TModel>(this IHtmlHelper<TModel> @this, Expression<Func<TModel, BlobViewModel>> property, object htmlAttributes = null)
        {
            return Context.Current.GetService<IFileUploadMarkupGenerator>()
                .Generate(@this, @this.ViewData.Model, property, htmlAttributes);
        }

        public static HtmlString CheckBoxesFor<TModel, TProperty>(this IHtmlHelper<TModel> @this, Expression<Func<TModel, TProperty>> property, IEnumerable<SelectListItem> selectList, object htmlAttributes = null) =>
            GenerateCheckBoxesFor(@this, property, selectList, htmlAttributes, setContainerId: true);

        static HtmlString GenerateCheckBoxesFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> property, IEnumerable<SelectListItem> selectList, object htmlAttributes, bool setContainerId)
        {
            var propertyInfo = property.GetProperty();

            var value = propertyInfo.GetValue(html.ViewData.Model);

            return html.GenerateCheckBoxes(propertyInfo.Name, value as IEnumerable, selectList, htmlAttributes, propertyInfo, setContainerId);
        }

        public static HtmlString CheckBoxes(this IHtmlHelper @this, string name, IEnumerable selectedItems, IEnumerable<SelectListItem> selectList, object htmlAttributes = null, PropertyInfo property = null)
        {
            return GenerateCheckBoxes(@this, name, selectedItems, selectList, htmlAttributes, property, setContainerId: true);
        }

        static HtmlString GenerateCheckBoxes(this IHtmlHelper html, string name, IEnumerable selectedItems, IEnumerable<SelectListItem> selectList, object htmlAttributes = null, PropertyInfo property = null, bool setContainerId = true)
        {
            var currentItems = new string[0];

            if (selectedItems != null)
            {
                if (selectedItems is string) currentItems = new[] { (string)selectedItems };
                else currentItems = (selectedItems as IEnumerable).Cast<object>().ExceptNull()
                .Select(x => ((x as IEntity)?.GetId()).ToStringOrEmpty().Or(x.ToString())).ToArray();
            }

            var settings = ToHtmlAttributes(htmlAttributes);

            var r = new StringBuilder();

            r.Append("<div class=\"checkbox-list\"");
            r.AppendIf($" id=\"{name}\"", setContainerId);
            r.AppendLine(">");

            var isRequiredProperty = property?.IsDefined(typeof(RequiredAttribute)) == true;

            var requiredValidationMessage = isRequiredProperty ? GetRequiredValidationMessage(property) : string.Empty;

            foreach (var item in selectList)
            {
                r.AddFormattedLine("<div{0}>", settings);

                var id = name + "_" + selectList.IndexOf(item);

                r.AppendLine($"<input type=\"checkbox\" id=\"{id}\" name=\"{name}\" value=\"{item.Value}\" class=\"form-check-input\"");

                if (currentItems.Contains(item.Value)) r.Append(" checked=\"checked\"");

                r.AppendIf(" disabled", item.Disabled);

                r.AppendIf($" data-val=\"true\" data-val-selection-required data-val-required=\"{requiredValidationMessage}\"", isRequiredProperty);

                r.AppendLine(">");

                r.AppendLine($"<label for=\"{id}\" class=\"form-check-label\">{item.Text}</label>");
                r.AppendLine("</div>");
            }

            r.AppendLine("</div>");

            return new HtmlString(r.ToString());
        }

        static string GetRequiredValidationMessage(PropertyInfo property)
        {
            var requiredAttribute = property?.GetCustomAttribute(typeof(RequiredAttribute)) as RequiredAttribute;

            if (requiredAttribute.ErrorMessage.HasValue()) return requiredAttribute.ErrorMessage;

            var propertyName = property.Name;

            if (property.IsDefined(typeof(DisplayNameAttribute)))
            {
                var displayAttribute = property?.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
                propertyName = displayAttribute?.DisplayName;
            }

            return $"The {propertyName} field is required.";
        }

        public static string ToHtmlAttributes(object htmlAttributes)
        {
            if (htmlAttributes == null) return string.Empty;

            var settings = htmlAttributes.GetType().GetProperties()
                .Select(x => new { name = x.Name.Replace("_", "-"), value = x.GetValue(htmlAttributes) }).ToList();

            var r = new StringBuilder();

            return settings.Select(x => x.name + "=\"" + x.value + "\"").ToString(" ").WithPrefix(" ");
        }

        // //START : frz:Should remove in next version

        // const int DEFAULT_VISIBLE_PAGES = 7;
        // [Obsolete("This method is obsolete. Call [Olive.Mvc.Pagination.Extensions.Pagination] instead. this method will be remove in next version.", error: false)]
        // public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, object htmlAttributes = null, string prefix = null) =>
        //    Pagination(html, paging, DEFAULT_VISIBLE_PAGES, htmlAttributes, prefix);

        // [Obsolete("This method is obsolete. Call [Olive.Mvc.Pagination.Extensions.Pagination] instead. this method will be remove in next version.", error: false)]
        // public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, int visiblePages, object htmlAttributes = null, string prefix = null) =>
        //    new PaginationRenderer(html, paging, visiblePages, htmlAttributes, prefix).Render();

        // //END : frz:Should remove in next version

        /// <summary>
        /// Will join this with other Mvc Html String items;
        /// </summary>
        public static IHtmlContent Concat(this IHtmlContent @this, IHtmlContent first, params IHtmlContent[] others)
        {
            var result = new HtmlContentBuilder();
            result.AppendHtml(@this);
            result.AppendHtml(first);
            others.Do(x => result.AppendHtml(x));

            return result;
        }

        public static IHtmlContent RegisterStartupActions(this IHtmlHelper @this)
        {
            var startupActions = @this.GetActionsJson().ToString().Unless("[]");

            var result = startupActions.HasValue() ? @this.Hidden("Startup.Actions", startupActions) : HtmlString.Empty;

            @this.ClearActionsJson();

            if (@this.Request().IsAjaxGet())
            {
                var title = Context.Current.Http().Items["Page.Title"].ToStringOrEmpty().Or(@this.ViewData["Title"].ToStringOrEmpty());
                result = result.Concat(@this.Hidden("page.meta.title", title));
            }

            return result;
        }

        public static HttpRequest Request(this IHtmlHelper @this) => @this?.ViewContext?.HttpContext?.Request;

        /// <summary>
        /// Creates a hidden field to contain the json data for the start-up actions.
        /// </summary>
        public static IHtmlContent StartupActionsJson(this IHtmlHelper @this)
        {
            if (!@this.Request().IsAjaxPost()) return null;

            var startupActions = @this.GetActionsJson().ToString().Unless("[]");

            if (startupActions.HasValue())
            {
                @this.ClearActionsJson();

                return @this.Hidden("Startup.Actions", startupActions);
            }

            return HtmlString.Empty;
        }

        public static HtmlString RunJavascript(this IHtmlHelper @this, string script, PageLifecycleStage stage = PageLifecycleStage.Init) =>
            RunJavascript(@this, script, script, stage);

        public static HtmlString RunJavascript(this IHtmlHelper @this, string key, string script, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            var actions = @this.ViewContext.HttpContext.JavascriptActions();

            // If already added, ignore:
            var exists = actions
                .Where(x => x.GetType().GetProperty("Script") != null)
                .Select(x => new { KeyProperty = x.GetType().GetProperty("Key"), Item = x })
                .Where(x => x.KeyProperty != null)
                .Any(x => x.KeyProperty.GetValue(x.Item).ToStringOrEmpty() == key);

            if (!exists)
                actions.Add(new { Script = script, Key = key, Stage = stage.ToString() });

            return HtmlString.Empty;
        }

        public static HtmlString RunJavascript(this IHtmlHelper @this, JavascriptService service, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            var actions = @this.ViewContext.HttpContext.JavascriptActions();

            var exists = actions.Any(x => x is JavascriptService j && j == service);

            if (!exists) actions.Add(service);

            return HtmlString.Empty;
        }

        public static HtmlString ReferenceScriptFile(this IHtmlHelper @this, string scriptUrl)
        {
            var context = @this.ViewContext.HttpContext;

            if (!(context.Items["MVC.Registered.Script.Files"] is List<string> items))
                context.Items["MVC.Registered.Script.Files"] = items = new List<string>();

            if (items.Lacks(scriptUrl)) items.Add(scriptUrl);

            return HtmlString.Empty;
        }

        /// <summary>
        /// Call this at the bottom of the layout file. It will register script tags to reference dynamically referenced script files.
        /// </summary>
        public static HtmlString RegisterDynamicScriptFiles(this IHtmlHelper @this)
        {
            var result = new StringBuilder();

            if (@this.ViewContext.HttpContext.Items["MVC.Registered.Script.Files"] is List<string> items)
                return new HtmlString(items.Select(f => $"<script type='text/javascript' src='{f}'></script>").ToLinesString());

            return HtmlString.Empty;
        }

        static void ClearActionsJson(this IHtmlHelper @this)
        {
            @this.ViewContext.HttpContext.ClearJavascriptActions();
        }
    }
}