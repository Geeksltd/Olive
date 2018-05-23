﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Olive.Entities;

namespace Olive
{
    partial class OliveWebExtensions
    {
        /// <summary>
        /// Gets the cookies sent by the client.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetCookies(this HttpRequest request)
        {
            if (request.Cookies == null) return Enumerable.Empty<KeyValuePair<string, string>>();
            return request.Cookies.AsEnumerable();
        }

        /// <summary>
        /// Gets the record with the specified type. The ID of the record will be read from QueryString[key].
        /// </summary>
        public static async Task<T> GetOrDefault<T>(this HttpRequest request, string key)
        {
            request = request ?? Olive.Context.Current.Request() ??
                    throw new InvalidOperationException("Request.GetOrDefault<T>() can only be called inside an Http context.");

            if (key == ".") key = "." + typeof(T).Name;

            if (!request.Has(key)) return default(T);

            try { return await request.DoGet<T>(key, throwIfNotFound: false); }
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
        public static Task<T> Get<T>(this HttpRequest request, string key)
            => DoGet<T>(request, key, throwIfNotFound: true);

        static async Task<T> DoGet<T>(this HttpRequest request, string key, bool throwIfNotFound)
        {
            if (typeof(T).Implements<IEntity>())
                return await GetEntity<T>(request, key, throwIfNotFound);

            return (T)Param(request, key).To(typeof(T));
        }

        /// <summary>
        /// Returns a string value specified in the request context (form, query string or route).
        /// </summary>
        public static string Param(this HttpRequest request, string key)
        {
            if (request.HasFormContentType && request.Form.ContainsKey(key))
                return request.Form[key].ToStringOrEmpty();

            if (request.Query.ContainsKey(key))
                return request.Query[key].ToStringOrEmpty();

            return (request.GetRouteValues()?[key]).ToStringOrEmpty();
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

        public static IEnumerable<Task<T>> GetList<T>(this HttpRequest request, string key) where T : class, IEntity => GetList<T>(request, key, ',');

        /// <summary>
        /// Gets a list of objects of which Ids come in query string.
        /// </summary>
        /// <param name="key">The key of the query string element containing ids.</param>
        /// <param name="seperator">The sepeerator of Ids in the query string value. The default will be comma (",").</param>
        public static IEnumerable<Task<T>> GetList<T>(this HttpRequest request, string key, char seperator) where T : class, IEntity
        {
            var ids = request.Param(key);
            if (ids.IsEmpty()) yield break;
            else
                foreach (var id in ids.Split(seperator))
                    yield return Context.Current.Database().Get<T>(id);
        }

        /// <summary>
        /// Finds the search keywords used by this user on Google that led to the current request.
        /// </summary>
        public static string FindGoogleSearchKeyword(this HttpRequest request)
        {
            var urlReferrer = request.Headers["Referer"].ToString();
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
        public static string GetIPAddress(this HttpRequest request) => request.HttpContext.Connection.RemoteIpAddress.ToString();

        /// <summary>
        /// Determines if the specified argument exists in the request (form, query string or route).
        /// </summary>
        public static bool Has(this HttpRequest request, string argument)
        {
            if (request.HasFormContentType && request.Form.ContainsKey(argument)) return true;
            else if (request.Query.ContainsKey(argument)) return true;
            else return (request.GetRouteValues()?[argument]).ToStringOrEmpty().HasValue();
        }

        public static RouteValueDictionary GetRouteValues(this HttpRequest request)
        {
            return Olive.Context.Current.ActionContext()?.RouteData?.Values;
        }

        /// <summary>
        /// Determines if the specified argument not exists in the request (query string or form).
        /// </summary>
        public static bool Lacks(this HttpRequest request, string argument) => !request.Has(argument);

        /// <summary>
        /// Gets the root of the requested website.
        /// </summary>
        public static string RootUrl(this HttpRequest request) => $"{request.Scheme}://{request.Host}/";

        /// <summary>
        /// Gets the raw url of the request.
        /// </summary>
        public static string ToRawUrl(this HttpRequest request) =>
            $"{request.PathBase}{request.Path}{request.QueryString}";

        public static string ToPathAndQuery(this HttpRequest request) =>
            $"{request.Path}{request.QueryString}";

        /// <summary>
        /// Gets the absolute Uri of the request.
        /// </summary>
        public static string ToAbsoluteUri(this HttpRequest request) =>
            $"{request.RootUrl().TrimEnd('/')}{request.PathBase}{request.Path}{request.QueryString}";

        /// <summary>
        /// Gets the absolute URL for a specified relative url.
        /// </summary>
        public static string GetAbsoluteUrl(this HttpRequest request, string relativeUrl) =>
            request.RootUrl() + relativeUrl.TrimStart("/");

        /// <summary>
        /// Determines whether this is an Ajax call.
        /// </summary>
        public static bool IsAjaxRequest(this HttpRequest request) => request.IsAjaxCall();

        /// <summary>
        /// Determines whether this is an Ajax call.
        /// </summary>
        public static bool IsAjaxCall(this HttpRequest request) => request.Headers["X-Requested-With"] == "XMLHttpRequest";

        /// <summary>
        /// Determines if this is a GET http request.
        /// </summary>
        public static bool IsGet(this HttpRequest request) => request.Method == System.Net.WebRequestMethods.Http.Get;

        /// <summary>
        /// Determines if this is a POST http request.
        /// </summary>
        public static bool IsPost(this HttpRequest request) => request.Method == System.Net.WebRequestMethods.Http.Post;

        /// <summary>
        /// Gets the currently specified return URL.
        /// </summary>
        public static string GetReturnUrl(this HttpRequest request)
        {
            var result = request.Param("ReturnUrl");

            if (result.IsEmpty()) return string.Empty;

            if (result.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                result.ToCharArray().ContainsAny('\'', '\"', '>', '<') ||
                result.ContainsAny(new[] { "//", ":" }, caseSensitive: false))
                throw new Exception("Invalid ReturnUrl.");

            return result;
        }

        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                else
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
                return true;

            return false;
        }
    }
}