using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Olive
{
    partial class OliveWebExtensions
    {
        /// <summary>
        /// Gets the cookies sent by the client.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetCookies(this HttpRequest @this)
        {
            if (@this.Cookies == null) return Enumerable.Empty<KeyValuePair<string, string>>();
            return @this.Cookies.AsEnumerable();
        }

        /// <summary>
        /// Gets the record with the specified type. The ID of the record will be read from QueryString[key].
        /// </summary>
        public static async Task<T> GetOrDefault<T>(this HttpRequest @this, string key)
        {
            @this = @this ?? Olive.Context.Current.Request() ??
                    throw new InvalidOperationException("Request.GetOrDefault<T>() can only be called inside an Http context.");

            if (key == ".") key = "." + typeof(T).Name;

            if (!@this.Has(key)) return default(T);

            try { return await @this.DoGet<T>(key, throwIfNotFound: false); }
            catch
            {
                // No Loging needed
                return default(T);
            }
        }

        /// <summary>
        /// Gets the data with the specified type from form, query string or route with the specified key.
        /// If the specified type is an entity, then the ID of that record will be read from the request and then fetched from database.
        /// </summary>
        public static Task<T> Get<T>(this HttpRequest @this, string key)
            => DoGet<T>(@this, key, throwIfNotFound: true);

        static async Task<T> DoGet<T>(this HttpRequest request, string key, bool throwIfNotFound)
        {
            if (typeof(T).Implements<IEntity>())
                return await GetEntity<T>(request, key, throwIfNotFound);

            return (T)Param(request, key).To(typeof(T));
        }

        /// <summary>
        /// Returns a string value specified in the request context (form, query string or route).
        /// </summary>
        public static string Param(this HttpRequest @this, string key)
        {
            if (@this.HasFormContentType && @this.Form.ContainsKey(key))
                return @this.Form[key].ToStringOrEmpty();

            if (@this.Query.ContainsKey(key))
                return @this.Query[key].ToStringOrEmpty();

            return (@this.GetRouteValues()?[key]).ToStringOrEmpty();
        }

        /// <summary>
        /// Gets the record with the specified type. The ID of the record will be read from form, query string or route with the specified key.
        /// </summary>
        static async Task<T> GetEntity<T>(this HttpRequest request, string key, bool throwIfNotFound = true)
        {
            request = request ?? Context.Current.Request() ??
                    throw new InvalidOperationException("Request.Get<T>() can only be called inside an Http context.");

            if (key == ".") key = "." + typeof(T).Name;

            var value = request.Param(key);
            if (value.IsEmpty()) return default(T);

            try { return (T)await Context.Current.Database().Get(value, typeof(T)); }
            catch (Exception ex)
            {
                if (throwIfNotFound)
                    throw new InvalidOperationException($"Loading a {typeof(T).FullName} from the page argument of '{key}' failed.", ex);
                else
                    return default(T);
            }
        }

        public static IEnumerable<Task<T>> GetList<T>(this HttpRequest @this, string key) where T : class, IEntity => GetList<T>(@this, key, ',');

        /// <summary>
        /// Gets a list of objects of which Ids come in query string.
        /// </summary>
        /// <param name="key">The key of the query string element containing ids.</param>
        /// <param name="seperator">The sepeerator of Ids in the query string value. The default will be comma (",").</param>
        public static IEnumerable<Task<T>> GetList<T>(this HttpRequest @this, string key, char seperator) where T : class, IEntity
        {
            var ids = @this.Param(key);
            if (ids.IsEmpty()) yield break;
            else
                foreach (var id in ids.Split(seperator))
                    yield return Context.Current.Database().Get<T>(id);
        }

        /// <summary>
        /// Finds the search keywords used by this user on Google that led to the current request.
        /// </summary>
        public static string FindGoogleSearchKeyword(this HttpRequest @this)
        {
            var urlReferrer = @this.Headers["Referer"].ToString();
            if (urlReferrer.IsEmpty()) return null;

            // Note: Only Google is supported for now:

            if (!urlReferrer.ToLower().Contains(".google.co"))
                return null;

            foreach (var possibleQuerystringKey in new[] { "q", "as_q" })
            {
                var queryString = urlReferrer.Split('?').Skip(1).FirstOrDefault();
                var query = queryString.TrimStart("?").Split('&').Trim().
                    FirstOrDefault(p => p.StartsWith(possibleQuerystringKey + "="));

                if (query.HasValue())
                    return HttpUtility.UrlDecode(query.Substring(1 + possibleQuerystringKey.Length));
            }

            return null;
        }

        /// <summary>
        /// Gets the actual IP address of the user considering the Proxy and other HTTP elements.
        /// </summary>
        public static string GetIPAddress(this HttpRequest @this) => @this.HttpContext.Connection.RemoteIpAddress.ToString();

        /// <summary>
        /// Determines if the specified argument exists in the request (form, query string or route).
        /// </summary>
        public static bool Has(this HttpRequest @this, string argument)
        {
            if (@this.HasFormContentType && @this.Form.ContainsKey(argument)) return true;
            else if (@this.Query.ContainsKey(argument)) return true;
            else return (@this.GetRouteValues()?[argument]).ToStringOrEmpty().HasValue();
        }

        public static RouteValueDictionary GetRouteValues(this HttpRequest @this)
        {
            return Olive.Context.Current.ActionContext()?.RouteData?.Values;
        }

        /// <summary>
        /// Determines if the specified argument not exists in the request (query string or form).
        /// </summary>
        public static bool Lacks(this HttpRequest @this, string argument) => !@this.Has(argument);

        /// <summary>
        /// Gets the root of the requested website.
        /// </summary>
        public static string RootUrl(this HttpRequest @this)
        {
            var forwarded = @this.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var scheme = forwarded.Or(@this.Scheme);

            return $"{scheme}://{@this.Host}/";
        }

        /// <summary>
        /// Gets the raw url of the request.
        /// </summary>
        public static string ToRawUrl(this HttpRequest @this) =>
            $"{@this.PathBase}{@this.Path}{@this.QueryString}";

        public static string ToPathAndQuery(this HttpRequest @this) =>
            $"{@this.Path}{@this.QueryString}";

        /// <summary>
        /// Gets the absolute Uri of the request.
        /// </summary>
        public static string ToAbsoluteUri(this HttpRequest @this) =>
            $"{@this.RootUrl().TrimEnd('/')}{@this.PathBase}{@this.Path}{@this.QueryString}";

        /// <summary>
        /// Gets the absolute URL for a specified relative url.
        /// </summary>
        public static string GetAbsoluteUrl(this HttpRequest @this, string relativeUrl) =>
            @this.RootUrl() + relativeUrl.TrimStart("/");

        /// <summary>
        /// Determines whether this is an Ajax call.
        /// </summary>
        public static bool IsAjaxRequest(this HttpRequest @this) => @this.IsAjaxCall();

        /// <summary>
        /// Determines whether this is an Ajax call.
        /// </summary>
        public static bool IsAjaxCall(this HttpRequest @this)
        {
            if (@this.Headers["X-Requested-With"] == "XMLHttpRequest") return true;

            if (@this.IsGet()) return false;

            return @this.Form[".Olive-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// Determines if this is a GET http request.
        /// </summary>
        public static bool IsGet(this HttpRequest @this) => @this.Method == System.Net.WebRequestMethods.Http.Get;

        /// <summary>
        /// Determines if this is a POST http request.
        /// </summary>
        public static bool IsPost(this HttpRequest @this) => @this.Method == System.Net.WebRequestMethods.Http.Post;

        /// <summary>
        /// Gets the currently specified return URL.
        /// </summary>
        public static string GetReturnUrl(this HttpRequest @this)
        {
            var result = @this.Param("ReturnUrl");

            if (result.IsEmpty()) return string.Empty;

            if (result.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                result.ToCharArray().ContainsAny('\'', '\"', '>', '<') ||
                result.ContainsAny(new[] { "//", ":" }, caseSensitive: false))
                throw new Exception("Invalid ReturnUrl.");

            return result;
        }

        public static bool IsLocal(this HttpRequest @this)
        {
            var connection = @this.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (IPAddress.IsLoopback(connection.RemoteIpAddress)) return true;

                if (Dns.GetHostAddresses(Dns.GetHostName()).Contains(connection.RemoteIpAddress))
                    return true;
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
                return true;

            return false;
        }
    }
}