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
using Olive.Services.Testing;
using Olive.Web;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        const int DEFAULT_VISIBLE_PAGES = 7;

        // public static UrlHelper GetUrlHelper(this IHtmlHelper html) => new UrlHelper(html.ViewContext.RequestContext);

        // /// <summary>
        // /// Renders the specified partial view as an HTML-encoded string.
        // /// </summary>
        // /// <param name="html">The HTML helper instance that this method extends.</param>
        // /// <param name="partialViewName">The name of the partial view to render.</param>
        // /// <param name="model">The view model for the partial view.</param>
        // /// <returns>The partial view that is rendered as an HTML-encoded string.</returns>
        // public static HtmlString Partial<T>(this IHtmlHelper html, string partialViewName, T model, bool skipAjaxPost) where T : IViewModel
        // {
        //    var request = HttpContext.Current.Request;
        //    if (skipAjaxPost && request.IsAjaxCall() && request.IsPost()) return HtmlString.Empty;

        //    if (model == null)
        //    {
        //        model = (html.ViewContext.Controller as Controller).Bind<T>();

        //        if (model == null)
        //            throw new Exception("The model object passed to Partial() cannot be null.");
        //    }

        //    return html.Partial(partialViewName, model);
        // }

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

                result.AppendHtml(html.RadioButton(propertyInfo.Name, item.Value, IsSelected(item, value), new { id = id }));

                result.AppendHtmlLine($"<label for=\"{id}\">{item.Text}</label>");

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

                r.AppendLine($"<input type=\"checkbox\" id=\"{id}\" name=\"{name}\" value=\"{item.Value}\"");

                if (currentItems.Contains(item.Value)) r.Append(" checked=\"checked\"");

                r.AppendIf(" disabled", item.Disabled);

                r.AppendIf($" data-val=\"true\" data-val-selection-required data-val-required=\"{requiredValidationMessage}\"", isRequiredProperty);

                r.AppendLine(">");

                r.AppendLine($"<label for=\"{id}\">{item.Text}</label>");
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

        public static HtmlString CollapsibleCheckBoxesFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> property,
            IEnumerable<SelectListItem> selectList,
            object htmlAttributes = null)
        {
            var name = property.GetProperty().Name;

            var itemsHtml = html.GenerateCheckBoxesFor(property, selectList, htmlAttributes: null, setContainerId: false).ToString();
            var attributes = ToHtmlAttributes(htmlAttributes);

            var result = $@"<div id=""{name}"" {attributes} class='form-control'>
                                    <div class='options-container'>
		                                <div class='toolbox'>
			                                <a class='select-all'>Select all</a> | <a class='remove-all'>Remove all</a>
		                                </div>
                                        <div class='items-list'>{itemsHtml}</div>
		                                <div class='selection-container'></div>
		                            </div>
                                    <div class='search-container'>
                                        <span class='fa fa-search'></span>
                                        <input class='form-control textbox' id=""{name}_Search""/>                                        
                                    </div>
                                    <div class='caption-container'>
                                        <span class='fa fa-chevron-down'></span>                                        
                                        <input class='form-control textbox' id=""{name}_Caption"">                                        
                                    </div>
                                </div>";

            return new HtmlString(result);
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

        // public static void RenderAction<TController>(this IHtmlHelper html, string action = "Index")
        // {
        //    html.RenderAction(action, typeof(TController).Name.TrimEnd("Controller"));
        // }

        // /// <summary>
        // /// Invokes the Index action method of the specified controller and returns the result as an HTML string.
        // /// <param name="queryParameters">An anonymous object containing query string / route values to pass.</param>
        // /// </summary>
        // public static HtmlString Action<TController>(this IHtmlHelper html, object queryParameters)
        // {
        //    return Action<TController>(html, "Index", queryParameters);
        // }

        // /// <summary>
        // /// Invokes the specified child action method of the specified controller and returns the result as an HTML string.
        // /// <param name="queryParameters">An anonymous object containing query string / route values to pass.</param>
        // /// </summary>
        // public static HtmlString Action<TController>(this IHtmlHelper html, string action = "Index", object queryParameters = null)
        // {
        //    return html.Action(action, typeof(TController).Name.TrimEnd("Controller"), queryParameters);
        // }

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

        public static HtmlString ResetDatabaseLink(this IHtmlHelper html)
        {
            if (!WebTestManager.IsTddExecutionMode()) return null;

            if (html.Request().IsAjaxCall()) return null;

            if (WebTestManager.IsSanityExecutionMode())
                html.RunJavascript("page.skipNewWindows();");

            return new HtmlString(WebTestManager.GetWebTestWidgetHtml(Context.Http.Request));
        }

        // /// <summary>
        // /// Creates a new Html helper for a new ViewModel object.
        // /// This enables to start from a new view context, so that normal Html helper methods (such as TextBoxFor, etc) yield the correct name attributes, use correct existing value, etc.
        // /// </summary>
        // public static HtmlHelper<TTarget> For<TSource, TTarget>(this IHtmlHelper<TSource> html, TTarget model)
        // {
        //    var container = new BasicViewDataContainer { ViewData = new ViewDataDictionary { Model = model } };

        //    var viewContext = new ViewContext(html.ViewContext, html.ViewContext.View, container.ViewData, html.ViewContext.Writer);

        //    return new HtmlHelper<TTarget>(viewContext, container);
        // }

        public static HtmlString RunJavascript(this IHtmlHelper html, string script, PageLifecycleStage stage = PageLifecycleStage.Init) =>
            RunJavascript(html, script, script, stage);

        public static HtmlString RunJavascript(this IHtmlHelper html, string key, string script, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            var actions = html.ViewContext.HttpContext.Items["JavascriptActions"] as List<object>;
            if (actions == null) html.ViewContext.HttpContext.Items["JavascriptActions"] = actions = new List<object>();

            // If already added, ignore:
            var exists = actions
                .Where(x => x.GetType().GetProperty("Script") != null)
                .Where(x => x.GetType().GetProperty("Key") != null)
                .Cast<dynamic>().Any(x => x.Key == key);

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