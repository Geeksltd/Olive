using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities;
using Olive.Entities.Data;
using Olive.Web;

namespace Olive.Mvc
{
    public static partial class OliveMvcExtensions
    {
        public static HtmlString Replace(this IHtmlContent content, string oldText, string newText) =>
            new HtmlString(GetString(content).KeepReplacing(oldText, newText));

        public static HtmlString PrefixName(this IHtmlContent content, string prefix)
        {
            var code = GetString(content)
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
                var declared = output.Attributes.FirstOrDefault(a => a.Name == att);
                if (declared == null) continue;

                output.Attributes.Remove(declared);
                output.Attributes.Add(att, newValue);
            }
        }

        /// <summary>
        /// Looks for a property named propertyName_Visible on this object. If it finds one and find it to be false it returns true.
        /// Otherwise false.
        /// </summary>
        public static bool IsInvisible(this IViewModel viewModel, string propertyName)
        {
            var visibleProperty = viewModel.GetType().GetProperty(propertyName + "_Visible");

            if (visibleProperty == null) return false;

            return !(bool)visibleProperty.GetValue(viewModel);
        }

        public static bool IsValid(this ModelStateDictionary modelState, IViewModel viewModel)
        {
            foreach (var item in modelState)
                if (viewModel.IsInvisible(item.Key))
                {
                    item.Value.Errors.Clear();
                    item.Value.ValidationState = ModelValidationState.Skipped;
                }

            return modelState.IsValid;
        }

        /// <summary>
        /// Gets all errors for all values in this model state instance.
        /// </summary>
        public static IEnumerable<string> GetErrors(this ModelStateDictionary modelState, bool errorStack = false)
        {
            foreach (var item in modelState.Values)
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
        public static HtmlString Raw(this string text) => new HtmlString(text.OrEmpty());

        /// <summary>
        /// Gets access to the current ViewBag.
        /// </summary>
        public static dynamic ViewBag(this HttpContext context) => (dynamic)context.Items["ViewBag"];

        /// <summary>
        /// Gets the name of the currently requested controller.
        /// </summary>
        public static string GetCurrentControllerName(this ActionContext context) =>
            context.RouteData.Values["controller"].ToString();

        public static T OrderBy<T>(this T query, ListSortExpression sort)
         where T : IDatabaseQuery
        {
            query.OrderBy(sort.Expression, sort.Descending);
            return query;
        }

        public static IMvcBuilder Mvc(this Context context) => context.GetService<IMvcBuilder>();

        public static TAttribute GetAttribute<TAttribute>(this ModelMetadata metadata) where TAttribute : System.Attribute
        {
            return (metadata as DefaultModelMetadata)?.Attributes.Attributes.OfType<TAttribute>().FirstOrDefault();
        }

        public static TUser Extract<TUser>(this ClaimsPrincipal user)
           where TUser : IEntity
        {
            return HttpContextCache.GetOrAdd("Olive.Principal.ExtractedUser", () =>
            {
                var id = user.GetId();
                if (id == null) return default(TUser);
                return Task.Factory.RunSync(() => Database.Instance.GetOrDefault<TUser>(id));
            });
        }

        public static HtmlString ToIcon(this Exception @this, string errorMessage = null)
        {
            if (errorMessage == null) errorMessage = @this.Message;

            return ($"<error title=\"{errorMessage.HtmlEncode()}\" class=\"soft-error-icon\" />").Raw();
        }
    }
}