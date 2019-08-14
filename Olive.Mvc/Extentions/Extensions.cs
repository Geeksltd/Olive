using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities;
using Olive.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public static partial class OliveMvcExtensions
    {
        public static HtmlString Replace(this IHtmlContent @this, string oldText, string newText) =>
            new HtmlString(GetString(@this).KeepReplacing(oldText, newText));

        public static HtmlString PrefixName(this IHtmlContent @this, string prefix)
        {
            var code = GetString(@this)
                .Replace(" name=\"", $" name=\"{prefix}.")
                .Replace(" id=\"", $" id=\"{prefix}.")
                .Replace(" for=\"", $" for=\"{prefix}.");

            return new HtmlString(code);
        }

        internal static string GetString(this IHtmlContent content)
        {
            var writer = new System.IO.StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);

            return writer.ToString();
        }

        internal static void ReplaceIdentificationAttributes(this Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output, string newValue)
        {
            foreach (var att in new[] { "name", "id", "for" })
            {
                var declared = output.Attributes.Where(a => a.Name == att).ToArray();
                if (declared.None()) continue;

                output.Attributes.Remove(declared);
                output.Attributes.Add(att, newValue);
            }
        }

        /// <summary>
        /// Looks for a property named propertyName_Visible on this object. If it finds one and find it to be false it returns true.
        /// Otherwise false.
        /// </summary>
        public static bool IsInvisible(this IViewModel @this, string propertyName)
        {
            var visibleProperty = @this.GetType().GetProperty(propertyName + "_Visible");

            if (visibleProperty == null) return false;

            return !(bool)visibleProperty.GetValue(@this);
        }

        public static bool IsValid(this ModelStateDictionary @this, IViewModel viewModel)
        {
            foreach (var item in @this)
                if (viewModel.IsInvisible(item.Key))
                {
                    item.Value.Errors.Clear();
                    item.Value.ValidationState = ModelValidationState.Skipped;
                }

            return @this.IsValid;
        }

        /// <summary>
        /// Gets all errors for all values in this model state instance.
        /// </summary>
        public static IEnumerable<string> GetErrors(this ModelStateDictionary @this, bool errorStack = false)
        {
            foreach (var item in @this.Values)
                foreach (var error in item.Errors)
                {
                    if (error.ErrorMessage.HasValue()) yield return error.ErrorMessage;
                    else if (error.Exception == null) continue;
                    else yield return errorStack ? error.Exception.ToLogString() : error.Exception.Message;
                }
        }

        /// <summary>
        /// Will convert this html string into a 
        /// </summary>
        public static HtmlString Raw(this string @this) => new HtmlString(@this.OrEmpty());

        /// <summary>
        /// Gets access to the current ViewBag.
        /// </summary>
        public static dynamic ViewBag(this HttpContext @this) => (dynamic)@this.Items["ViewBag"];

        /// <summary>
        /// Gets the name of the currently requested controller.
        /// </summary>
        public static string GetCurrentControllerName(this ActionContext @this) =>
            @this.RouteData.Values["controller"].ToString();

        public static T OrderBy<T>(this T query, ListSortExpression sort)
         where T : IDatabaseQuery
        {
            query.OrderBy(sort.Expression, sort.Descending);
            return query;
        }

        /// <summary>
        /// Sort.Expression null safe version of the OrderBy(ListSortExpression).
        /// </summary>
        public static T Sort<T>(this T @this, ListSortExpression sort)
            where T : IDatabaseQuery => sort.Expression.HasValue() ? @this.OrderBy(sort) : @this;

        public static IMvcBuilder Mvc(this Context @this) => @this.GetService<IMvcBuilder>();

        public static TAttribute GetAttribute<TAttribute>(this ModelMetadata @this) where TAttribute : System.Attribute
        {
            return (@this as DefaultModelMetadata)?.Attributes.Attributes.OfType<TAttribute>().FirstOrDefault();
        }

        public static TUser Extract<TUser>(this ClaimsPrincipal @this)
           where TUser : IEntity
        {
            return HttpContextCache.GetOrAdd("Olive.Principal.ExtractedUser", () =>
            {
                var id = @this.GetId();
                if (id.IsEmpty()) return default(TUser);
                return Task.Factory.RunSync(() => Context.Current.Database().GetOrDefault<TUser>(id));
            });
        }

        public static HtmlString ToIcon(this Exception @this, string errorMessage = null)
        {
            if (errorMessage.IsEmpty()) errorMessage = @this.Message;

            return ($"<error title=\"{errorMessage.HtmlEncode()}\" class=\"soft-error-icon\" />").Raw();
        }

        internal static JavascriptActions JavascriptActions(this HttpContext context)
        {
            return (JavascriptActions)(context.Items["JavascriptActions"] ??
                  (context.Items["JavascriptActions"] = new JavascriptActions()));
        }

        internal static void ClearJavascriptActions(this HttpContext context)
        {
            context.Items["JavascriptActions"] = new JavascriptActions();
        }

        internal static string OnLoaded(this IEnumerable<JavascriptDependency> dependencies, string javaScript)
        {
            if (dependencies.Any())
            {
                var deps = dependencies.Select(x => "'" + x.Url + "'").ToString(", ");
                javaScript = $"requirejs([{deps}], function () {{\n {javaScript}\n}});";
            }

            return javaScript;
        }

        /// <summary>
        /// Gets the specified view model object from the current context's action parameters.
        /// </summary>
        /// <param name="key">If there is no item with the specified key, it will also attempt the special key 'info'.</param>
        public static T GetViewModel<T>(this ActionExecutingContext @this, string key = "info")
            where T : class, IViewModel
        {
            var result = @this?.ActionArguments?.GetOrDefault(key);
            if (result != null) return (T)result;

            if (key != "info")
                return @this?.ActionArguments?.GetOrDefault("info") as T;

            return null;
        }
    }
}