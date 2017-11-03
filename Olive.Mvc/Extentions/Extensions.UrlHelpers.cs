using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Olive.Entities;
using Olive.Web;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        static ConcurrentDictionary<string, List<RouteTemplate>> IndexActionRoutes = new ConcurrentDictionary<string, List<RouteTemplate>>();

        public static string Current(this IUrlHelper url)
        {
            var result = url.ActionContext?.HttpContext.Request.GetValue("current.request.url");

            if (result.IsEmpty()) result = Context.Http?.Request.GetValue("current.request.url");

            if (result.HasValue())
            {
                if (!url.IsLocalUrl(result)) throw new Exception("Invalid injected current url.");
            }
            else
            {
                result = url.ActionContext?.HttpContext.Request.ToRawUrl();

                if (result.IsEmpty()) result = Context.Http?.Request.ToRawUrl();
            }

            return result;
        }

        public static string Current(this IUrlHelper url, object queryParameters)
        {
            if (queryParameters == null) return Current(url);

            var settings = queryParameters.GetType().GetProperties()
                    .ToDictionary(x => x.Name, x => x.GetValue(queryParameters).ToStringOrEmpty());

            return Current(url, settings);
        }

        public static string Current(this IUrlHelper url, IDictionary<string, string> queryParameters)
        {
            var result = url.CurrentUri().RemoveEmptyQueryParameters();

            if (queryParameters == null) queryParameters = new Dictionary<string, string>();

            foreach (var item in queryParameters)
                result = result.RemoveQueryString(item.Key).AddQueryString(item.Key, item.Value);

            return result.PathAndQuery;
        }

        public static Uri CurrentUri(this IUrlHelper urlHelper)
        {
            var url = urlHelper.Current();
            if (url.StartsWith("http")) return url.AsUri().RemoveEmptyQueryParameters();
            return ($"http://domain.com{url}").AsUri().RemoveEmptyQueryParameters();
        }

        public static string QueryString(this IUrlHelper url) => url.ActionContext.HttpContext.Request.QueryString.ToString();

        public static string ReturnUrl(this IUrlHelper urlHelper)
        {
            var url = urlHelper.ActionContext.HttpContext.Request.GetValue("ReturnUrl");
            if (url.IsEmpty()) return string.Empty;

            if (urlHelper.IsLocalUrl(url)) return url;

            throw new Exception(url + " is not a valid ReturnUrl as it's external and so unsafe.");
        }

        /// <summary>
        /// Returns the specified actionUrl. But it first adds the current route and query string query parameters, all as query string.
        /// </summary>
        public static string ActionWithQuery(this IUrlHelper url, string actionUrl, IEntity listItem = null) =>
            url.ActionWithQuery(actionUrl, new { list_item = listItem });

        public static string ActionWithQuery(this IUrlHelper url, string actionUrl, object query)
        {
            var data = url.ActionContext.GetRequestParameters();

            if (query != null)
            {
                var queryData = query.GetType().GetProperties()
                    .ToDictionary(p => p.Name.ToLower().Replace("_", "."), p =>
                    {
                        var value = p.GetValue(query);
                        if (value is IEntity)
                            return (value as IEntity).GetId().ToStringOrEmpty();
                        return value.ToStringOrEmpty();
                    });

                foreach (var item in queryData.Where(x => x.Value.HasValue())) data[item.Key] = item.Value;
            }

            var queryString = data.Where(x => x.Value.HasValue()).Select(x => x.Key + "=" + WebUtility.UrlEncode(x.Value)).ToString("&");

            return url.Content("~/" + actionUrl + queryString.WithPrefix("?").Replace("?&", "?"));
        }

        /// <summary>
        /// Determines if a request parameter (route or query string) value exists for the specified key.
        /// </summary>
        public static bool Has(this ActionContext actionContext, string key) => actionContext.Param(key).HasValue();

        /// <summary>
        /// Determines if a request parameter (route or query string) value does not exists for the specified key.
        /// </summary>
        public static bool Lacks(this ActionContext actionContext, string key) => !actionContext.Has(key);

        /// <summary>
        /// Will get the value for the specified key in the current request whether it comes from Route or Query String.
        /// </summary>
        public static string Param(this ActionContext actionContext, string key)
        {
            if (key.IsEmpty()) throw new ArgumentNullException(nameof(key));

            return actionContext.GetRequestParameters()
                .FirstOrDefault(x => x.Key.OrEmpty().ToLower() == key.ToLower())
                .Get(x => x.Value);
        }

        public static Task<IEnumerable<T>> GetList<T>(this HttpRequest request, string key, char separator = ',') where T : IEntity
        {
            return request.GetValue(key).OrEmpty().Split(separator).Trim().Select(x => Entity.Database.Get<T>(x)).AwaitAll();
        }

        public static Dictionary<string, string> GetRequestParameters(this ActionContext actionContext)
        {
            var result = actionContext.RouteData.Values.ToDictionary(x => x.Key, x => x.Value.ToStringOrEmpty());

            result.Remove("controller");
            result.Remove("action");

            var byQuerystring = actionContext.HttpContext.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            byQuerystring = byQuerystring.Except(x => result.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            result.Add(byQuerystring);

            return result;
        }

        public static object RouteValue(this IUrlHelper url, string key) => url.ActionContext.RouteData.Values[key];

        static List<RouteTemplate> FindIndexRouteTemplates(string controllerName)
        {
            var relevantAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.References(Assembly.GetExecutingAssembly())).ToList();

            var types = relevantAssemblies.SelectMany(a => a.GetTypes().Where(t => t.Name == controllerName))
                         .ExceptNull()
                         .Where(x => x.InhritsFrom(typeof(ControllerBase))).ToList();

            if (types.None())
                throw new Exception("Controller class not found: " + controllerName);

            if (types.HasMany())
                throw new Exception("Multiple Controller classes found: " + types.Select(x => x.AssemblyQualifiedName).ToLinesString());

            var type = types.Single();

            var indexAction = type.GetMethods().Where(x => x.Name == "Index").ToList();

            if (indexAction.None()) throw new Exception(type.FullName + " has no Index method.");

            if (indexAction.HasMany()) throw new Exception(type.FullName + " has multiple Index methods.");

            var attributes = indexAction.First().GetCustomAttributes<RouteAttribute>();

            if (attributes.None()) throw new Exception(type.FullName + ".Index() has no Route attribute.");

            return attributes.Select(x => new RouteTemplate(x.Template)).ToList();
        }

        public static string Index<TController>(this IUrlHelper url, object routeValues = null) where TController : Microsoft.AspNetCore.Mvc.Controller =>
            url.Index(typeof(TController).Name.TrimEnd("Controller"), routeValues);

        public static string Index(this IUrlHelper url, string controllerName, object routeValues = null)
        {
            var routeParameters = new Dictionary<string, string>();

            if (routeValues != null)
                routeParameters = routeValues.GetType().GetProperties()
                    .ToDictionary(p => p.Name.ToCamelCaseId(), p => p.GetValue(routeValues).ToStringOrEmpty());

            return url.Index(controllerName, routeParameters);
        }

        public static string Index(this IUrlHelper url, string controllerName, Dictionary<string, string> routeParameters)
        {
            if (!controllerName.EndsWith("Controller")) controllerName += "Controller";

            var routeTemplates = IndexActionRoutes.GetOrAdd(controllerName, FindIndexRouteTemplates);

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

        public static string MergeRoute(this IUrlHelper url, string routeTemplate, Dictionary<string, string> routeData)
        {
            if (routeTemplate.IsEmpty())
                throw new ArgumentNullException(nameof(routeTemplate));

            return new RouteTemplate(routeTemplate).Merge(routeData);
        }
    }
}