using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        static IDatabase Database => Context.Current.Database();

        public static string Current(this IUrlHelper @this)
        {
            var result = @this.ActionContext?.HttpContext.Request.Param("current.request.url");

            if (result.IsEmpty()) result = Context.Current.Request().Param("current.request.url");

            if (result.HasValue())
            {
                if (!@this.IsLocalUrl(result) && result.Contains("//"))
                    throw new Exception("Invalid injected current url.");
            }
            else
            {
                result = @this.ActionContext?.HttpContext.Request.ToRawUrl();

                if (result.IsEmpty()) result = Context.Current.Request().ToRawUrl();
            }

            return result;
        }

        public static string Current(this IUrlHelper @this, object queryParameters)
        {
            if (queryParameters == null) return Current(@this);

            var settings = queryParameters.GetType().GetProperties()
                    .ToDictionary(x => x.Name, x => x.GetValue(queryParameters).ToStringOrEmpty());

            return Current(@this, settings);
        }

        public static string Current(this IUrlHelper @this, IDictionary<string, string> queryParameters)
        {
            if (queryParameters == null) queryParameters = new Dictionary<string, string>();

            var result = @this.CurrentUri().RemoveEmptyQueryParameters();
            foreach (var item in queryParameters)
                result = result.RemoveQueryString(item.Key).AddQueryString(item.Key, item.Value);

            return result.PathAndQuery;
        }

        public static Uri CurrentUri(this IUrlHelper @this)
        {
            var url = @this.Current();
            if (url.StartsWith("http")) return url.AsUri().RemoveEmptyQueryParameters();
            return ($"http://domain.com{url}").AsUri().RemoveEmptyQueryParameters();
        }

        public static string QueryString(this IUrlHelper @this) => @this.ActionContext.HttpContext.Request.QueryString.ToString();

        public static string ReturnUrl(this IUrlHelper @this)
        {
            var url = @this.ActionContext.HttpContext.Request.Param("ReturnUrl");
            if (url.IsEmpty()) return string.Empty;

            if (@this.IsLocalUrl(url)) return url;

            throw new Exception(url + " is not a valid ReturnUrl as it's external and so unsafe.");
        }

        /// <summary>
        /// Returns the specified actionUrl. But it first adds the current route and query string query parameters, all as query string.
        /// </summary>
        public static string ActionWithQuery(this IUrlHelper @this, string actionUrl, IEntity listItem = null) =>
            @this.ActionWithQuery(actionUrl, new { list_item = listItem });

        static Dictionary<string, string> SerializeQuery(object item)
        {
            var result = new Dictionary<string, string>();

            void serialize(object container, string key, object value)
            {
                if (value is IEntity entity)
                {
                    if (TransientEntityAttribute.IsTransient(entity.GetType()))
                    {
                        foreach (var pp in GetSerializableProperties(entity.GetType()))
                            serialize(value, key + "." + pp.Name, pp.GetValue(entity));
                    }
                    else
                    {
                        result[key] = entity.GetId().ToStringOrEmpty();
                    }
                }
                else
                {
                    result[key] = value.ToStringOrEmpty();
                }
            }

            foreach (var p in item.GetType().GetProperties())
            {
                var key = p.Name.ToLower().Replace("_", ".");
                var value = p.GetValue(item);
                serialize(item, key, value);
            }

            return result;
        }

        static IEnumerable<PropertyInfo> GetSerializableProperties(Type type)
        {
            foreach (var p in type.GetProperties())
            {
                if (!p.CanRead) continue;
                if (!p.CanWrite) continue;

                if (type.IsA<IEntity>())
                    if (p.Name.IsAnyOf(nameof(Entity.IsNew), nameof(Entity.IsMarkedSoftDeleted))) continue;

                yield return p;
            }
        }

        public static string ActionWithQuery(this IUrlHelper @this, string actionUrl, object query)
        {
            var data = @this.ActionContext.GetRequestParameters();

            if (query != null)
            {
                foreach (var item in SerializeQuery(query).Where(x => x.Value.HasValue()))
                    data[item.Key] = item.Value;
            }

            var queryString = data.Where(x => x.Value.HasValue()).Select(x => x.Key + "=" + WebUtility.UrlEncode(x.Value)).ToString("&");

            return @this.Content("~/" + actionUrl + queryString.WithPrefix("?").Replace("?&", "?"));
        }

        /// <summary>
        /// Determines if a request parameter (route or query string) value exists for the specified key.
        /// </summary>
        public static bool Has(this ActionContext @this, string key) => @this.Param(key).HasValue();

        /// <summary>
        /// Determines if a request parameter (route or query string) value does not exists for the specified key.
        /// </summary>
        public static bool Lacks(this ActionContext @this, string key) => !@this.Has(key);

        /// <summary>
        /// Will get the value for the specified key in the current request whether it comes from Route or Query String.
        /// </summary>
        public static string Param(this ActionContext @this, string key)
        {
            if (key.IsEmpty()) throw new ArgumentNullException(nameof(key));

            return @this.GetRequestParameters()
                .FirstOrDefault(x => x.Key.OrEmpty().ToLower() == key.ToLower()).Value;
        }

        public static Task<IEnumerable<T>> GetList<T>(this HttpRequest @this, string key, char separator = ',') where T : IEntity
        {
            return @this.Param(key).OrEmpty().Split(separator).Trim().SelectAsync(x => Database.Get<T>(x));
        }

        public static Dictionary<string, string> GetRequestParameters(this ActionContext @this)
        {
            var result = @this.RouteData.Values.ToDictionary(x => x.Key, x => x.Value.ToStringOrEmpty());

            result.Remove("controller");
            result.Remove("action");

            var byQuerystring = @this.HttpContext.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            byQuerystring = byQuerystring.Except(x => result.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            result.Add(byQuerystring);

            return result;
        }

        public static object RouteValue(this IUrlHelper @this, string key) => @this.ActionContext.RouteData.Values[key];

        public static string Index<TController>(this IUrlHelper @this, object routeValues = null) where TController : Microsoft.AspNetCore.Mvc.Controller =>
            @this.Index(typeof(TController).Name.TrimEnd("Controller"), routeValues);

        public static string Index(this IUrlHelper @this, string controllerName, object routeValues = null)
        {
            var routeParameters = new Dictionary<string, string>();

            if (routeValues != null)
                routeParameters = routeValues.GetType().GetProperties()
                    .ToDictionary(p => p.Name.ToCamelCaseId(), p => p.GetValue(routeValues).ToStringOrEmpty());

            return @this.Index(controllerName, routeParameters);
        }

        public static string Index(this IUrlHelper @this, string controllerName, Dictionary<string, string> routeParameters)
        {
            if (!controllerName.EndsWith("Controller")) controllerName += "Controller";

            var routeTemplates = RouteTemplates.GetTemplates(controllerName);

            var bestRouteMatch = FindBestRouteMatch(routeTemplates, routeParameters);

            if (bestRouteMatch == null)
            {
                var message = $"Failed to evaluate: @Url.Index(\"{controllerName.TrimEnd("Controller")}\"" +
                    routeParameters.Select(x => x.Key + "= «" + x.Value + "»").ToString(", ").WithWrappers(", new { ", "}") +
                    ")\r\n\r\n";

                message += "Destination route pattern(s) don't match the provided parameters.\r\n\r\n";
                message += "Destination route pattern(s): " + routeTemplates.ToLinesString();

                throw new Exception(message);
            }

            try
            {
                return bestRouteMatch.Merge(routeParameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create the URL for pattern: '" + bestRouteMatch + "' with the provided data: {" +
                    routeParameters.Select(x => x.Key + ":" + x.Value).ToString(" , ") + "}", ex);
            }
        }

        static RouteTemplate FindBestRouteMatch(IEnumerable<RouteTemplate> templates, Dictionary<string, string> providedParameters)
        {
            templates = templates.Where(t => t.IsMatch(providedParameters)).ToList();

            return templates
                // Exact number of parameters takes priority.
                .OrderByDescending(t => t.FindMatchingParameters(providedParameters).Count())

                // Then the one with highest number of parameters.
                .ThenBy(t => t.Parameters.Count - t.FindMatchingParameters(providedParameters).Count())
                .FirstOrDefault();
        }

        public static string MergeRoute(this IUrlHelper @this, string routeTemplate, Dictionary<string, string> routeData)
        {
            if (routeTemplate.IsEmpty())
                throw new ArgumentNullException(nameof(routeTemplate));

            return new RouteTemplate(routeTemplate).Merge(routeData);
        }
    }
}