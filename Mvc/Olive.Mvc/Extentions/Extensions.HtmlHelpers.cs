using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Olive;
using Olive.Entities;
using Olive.Web;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        const int DEFAULT_VISIBLE_PAGES = 7;

        public static HtmlString ToJson(this IHtmlHelper html, object obj)
        {
            if (obj == null) return new HtmlString("[]");

            return new HtmlString(JsonConvert.SerializeObject(obj));
        }

        public static HtmlString GetActionsJson(this IHtmlHelper html)
        {
            var data = html.ViewContext.HttpContext.Items["JavascriptActions"] as object;

            return html.ToJson(data);
        }

        public static IHtmlContent RadioButtonsFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> property, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            var propertyInfo = property.GetProperty();

            var value = propertyInfo.GetValue(html.ViewData.Model);

            if (value is IEntity) value = (value as IEntity).GetId();

            var settings = ToHtmlAttributes(htmlAttributes);

            var result = new HtmlContentBuilder();

            result.AppendHtmlLine($"<div class=\"radio-list\" id=\"{propertyInfo.Name}\">");

            foreach (var item in selectList)
            {
                result.AppendHtmlLine($"<div{settings}>");

                var id = propertyInfo.Name + "_" + selectList.IndexOf(item);

                result.AppendHtml(html.RadioButton(propertyInfo.Name, item.Value, IsSelected(item, value),
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

        public static IHtmlContent FileUploadFor<TModel>(this IHtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<Blob>>> property, object htmlAttributes = null) =>
            new DefaultFileUploadMarkupGenerator().Generate(html, html.ViewData.Model, property, htmlAttributes);

        public static IHtmlContent FileUploadFor<TModel>(this IHtmlHelper<TModel> html, Expression<Func<TModel, Blob>> property, object htmlAttributes = null) =>
            new DefaultFileUploadMarkupGenerator().Generate(html, html.ViewData.Model, property, htmlAttributes);

        public static HtmlString CheckBoxesFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> property, IEnumerable<SelectListItem> selectList, object htmlAttributes = null) =>
            GenerateCheckBoxesFor(html, property, selectList, htmlAttributes, setContainerId: true);

        static HtmlString GenerateCheckBoxesFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> property, IEnumerable<SelectListItem> selectList, object htmlAttributes, bool setContainerId)
        {
            var propertyInfo = property.GetProperty();

            var value = propertyInfo.GetValue(html.ViewData.Model);

            return html.GenerateCheckBoxes(propertyInfo.Name, value as IEnumerable, selectList, htmlAttributes, propertyInfo, setContainerId);
        }

        public static HtmlString CheckBoxes(this IHtmlHelper html, string name, IEnumerable selectedItems, IEnumerable<SelectListItem> selectList, object htmlAttributes = null, PropertyInfo property = null)
        {
            return GenerateCheckBoxes(html, name, selectedItems, selectList, htmlAttributes, property, setContainerId: true);
        }

        static HtmlString GenerateCheckBoxes(this IHtmlHelper html, string name, IEnumerable selectedItems, IEnumerable<SelectListItem> selectList, object htmlAttributes = null, PropertyInfo property = null, bool setContainerId = true)
        {
            var currentItems = new string[0];

            if (selectedItems != null)
            {
                if (selectedItems is string) currentItems = new[] { (string)selectedItems };
                else currentItems = (selectedItems as IEnumerable).Cast<object>().ExceptNull()
                .Select(x => (x as IEntity).Get(b => b.GetId()).ToStringOrEmpty().Or(x.ToString())).ToArray();
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

        internal static string ToHtmlAttributes(object htmlAttributes)
        {
            if (htmlAttributes == null) return string.Empty;

            var settings = htmlAttributes.GetType().GetProperties()
                .Select(x => new { name = x.Name.Replace("_", "-"), value = x.GetValue(htmlAttributes) }).ToList();

            var r = new StringBuilder();

            return settings.Select(x => x.name + "=\"" + x.value + "\"").ToString(" ").WithPrefix(" ");
        }

        public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, object htmlAttributes = null, string prefix = null) =>
            Pagination(html, paging, DEFAULT_VISIBLE_PAGES, htmlAttributes, prefix);

        public static HtmlString Pagination(this IHtmlHelper html, ListPagination paging, int visiblePages, object htmlAttributes = null, string prefix = null) =>
            new PaginationRenderer(html, paging, visiblePages, htmlAttributes, prefix).Render();

        /// <summary>
        /// Will join this with other Mvc Html String items;
        /// </summary>
        public static IHtmlContent Concat(this IHtmlContent me, IHtmlContent first, params IHtmlContent[] others)
        {
            var result = new HtmlContentBuilder();
            result.AppendHtml(me);
            result.AppendHtml(first);
            others.Do(x => result.AppendHtml(x));

            return result;
        }

        public static IHtmlContent RegisterStartupActions(this IHtmlHelper html)
        {
            var startupActions = html.GetActionsJson().ToString().Unless("[]");

            var result = startupActions.HasValue() ? html.Hidden("Startup.Actions", startupActions) : HtmlString.Empty;

            var request = html.ViewContext.HttpContext.Request;

            if (request.IsAjaxGet())
            {
                var title = Context.Http.Items["Page.Title"].ToStringOrEmpty().Or(html.ViewData["Title"].ToStringOrEmpty());
                result = result.Concat(html.Hidden("page.meta.title", title));
            }

            return result;
        }

        public static HttpRequest Request(this IHtmlHelper html) =>
            html.ViewContext.HttpContext.Request;

        /// <summary>
        /// Creates a hidden field to contain the json data for the start-up actions.
        /// </summary>
        public static IHtmlContent StartupActionsJson(this IHtmlHelper html)
        {
            if (!html.Request().IsAjaxPost()) return null;

            var startupActions = html.GetActionsJson().ToString().Unless("[]");

            if (startupActions.HasValue())
                return html.Hidden("Startup.Actions", startupActions);

            return HtmlString.Empty;
        }

        public static HtmlString RunJavascript(this IHtmlHelper html, string script, PageLifecycleStage stage = PageLifecycleStage.Init) =>
            RunJavascript(html, script, script, stage);

        public static HtmlString RunJavascript(this IHtmlHelper html, string key, string script, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            var actions = html.ViewContext.HttpContext.Items["JavascriptActions"] as List<object>;
            if (actions == null) html.ViewContext.HttpContext.Items["JavascriptActions"] = actions = new List<object>();

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

        public static HtmlString ReferenceScriptFile(this IHtmlHelper html, string scriptUrl)
        {
            var items = html.ViewContext.HttpContext.Items["MVC.Registered.Script.Files"] as List<string>;
            if (items == null) html.ViewContext.HttpContext.Items["MVC.Registered.Script.Files"] = items = new List<string>();

            if (items.Lacks(scriptUrl)) items.Add(scriptUrl);

            return HtmlString.Empty;
        }

        /// <summary>
        /// Call this at the bottom of the layout file. It will register script tags to reference dynamically referenced script files.
        /// </summary>
        public static HtmlString RegisterDynamicScriptFiles(this IHtmlHelper html)
        {
            var result = new StringBuilder();

            if (html.ViewContext.HttpContext.Items["MVC.Registered.Script.Files"] is List<string> items)
                return new HtmlString(items.Select(f => $"<script type='text/javascript' src='{f}'></script>").ToLinesString());

            return HtmlString.Empty;
        }
    }
}