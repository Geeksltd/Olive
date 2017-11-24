using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Olive.Entities;

namespace Olive.Web
{
    public static partial class OliveExtensions
    {
        const int HTTP_PORT_NUMBER = 80;

        const int HTTPS_PORT_NUMBER = 443;

        const int MOVED_PERMANENTLY_STATUS_CODE = 301;

        const int DEFAULT_DOWNLOAD_TIMEOUT = 60000;

        static readonly Range<uint>[] PrivateIpRanges = new[] {
             //new Range<uint>(0u, 50331647u),              // 0.0.0.0 to 2.255.255.255
             new Range<uint>(167772160u, 184549375u),     // 10.0.0.0 to 10.255.255.255
             new Range<uint>(2130706432u, 2147483647u),   // 127.0.0.0 to 127.255.255.255
             new Range<uint>(2851995648u, 2852061183u),   // 169.254.0.0 to 169.254.255.255
             new Range<uint>(2886729728u, 2887778303u),   // 172.16.0.0 to 172.31.255.255
             new Range<uint>(3221225984u, 3221226239u),   // 192.0.2.0 to 192.0.2.255
             new Range<uint>(3232235520u, 3232301055u),   // 192.168.0.0 to 192.168.255.255
             new Range<uint>(4294967040u, 4294967295u)    // 255.255.255.0 to 255.255.255.255
        };

        /// <summary>
        /// Adds the specified query string setting to this Url.
        /// </summary>
        public static Uri AddQueryString(this Uri url, string key, string value)
        {
            var qs = url.GetQueryString();

            qs.RemoveWhere(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            qs.Add(key, value);

            return url.ReplaceQueryString(qs);
        }

        /// <summary>
        /// Gets the query string parameters of this Url.
        /// </summary>
        public static Dictionary<string, string> GetQueryString(this Uri url)
        {
            var entries = HttpUtility.ParseQueryString(url.Query);
            return entries.AllKeys.ExceptNull().ToDictionary(a => a.ToLower(), a => entries[a]);
        }

        #region Request.Get

        /// <summary>
        /// Gets the cookies sent by the client.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetCookies(this HttpRequest request)
        {
            if (request.Cookies == null) return Enumerable.Empty<KeyValuePair<string, string>>();
            return request.Cookies.AsEnumerable();
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
            if (Context.Request != request)
                throw new Exception("The given request is not match with ActionContext`s request.");

            if (request.HasFormContentType && request.Form.ContainsKey(key))
                return request.Form[key].ToStringOrEmpty();

            if (request.Query.ContainsKey(key))
                return request.Query[key].ToStringOrEmpty();

            return request.GetRouteValues()[key].ToStringOrEmpty();
        }

        /// <summary>
        /// Gets the record with the specified type. The ID of the record will be read from form, query string or route with the specified key.
        /// </summary>
        static async Task<T> GetEntity<T>(this HttpRequest request, string key, bool throwIfNotFound = true)
        {
            if (request == null)
            {
                if (Context.Http != null)
                    request = Context.Request;
                else
                    throw new InvalidOperationException("Request.Get<T>() can only be called inside an Http context.");
            }

            if (key == ".") key = "." + typeof(T).Name;

            var value = request.Param(key);
            if (value.IsEmpty()) return default(T);

            try { return (T)await Entity.Database.Get(value, typeof(T)); }
            catch (Exception ex)
            {
                if (throwIfNotFound)
                    throw new InvalidOperationException($"Loading a {typeof(T).FullName} from the page argument of '{key}' failed.", ex);
                else
                    return default(T);
            }
        }

        #endregion

        #region Request.GetOrDefault

        /// <summary>
        /// Gets the record with the specified type. The ID of the record will be read from QueryString[key].
        /// </summary>
        public static async Task<T> GetOrDefault<T>(this HttpRequest request, string key)
        {
            request = request ?? Context.Request ??
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

        #endregion

        public static IEnumerable<Task<T>> GetList<T>(this HttpRequest request, string key) where T : class, IEntity => GetList<T>(request, key, ',');

        /// <summary>
        /// Gets a list of objects of which Ids come in query string.
        /// </summary>
        /// <param name="key">The key of the query string element containing ids.</param>
        /// <param name="seperator">The sepeerator of Ids in the query string value. The default will be comma (",").</param>
        public static IEnumerable<Task<T>> GetList<T>(this HttpRequest request, string key, char seperator) where T : class, IEntity
        {
            var ids = request.Param(key);
            if (ids.IsEmpty())
                yield break;
            else
                foreach (var id in ids.Split(seperator))
                    yield return Entity.Database.Get<T>(id);
        }

        /// <summary>
        /// Finds the search keywords used by this user on Google that led to the current request.
        /// </summary>
        public static string FindSearchKeyword(this HttpRequest request)
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

        #region Private IPs

        /// <summary>
        /// Determines if the given ip address is in any of the private IP ranges
        /// </summary>
        public static bool IsPrivateIp(string address)
        {
            if (address.IsEmpty()) return false;

            var bytes = IPAddress.Parse(address).GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            var ip = BitConverter.ToUInt32(bytes, 0);

            return PrivateIpRanges.Any(range => range.Contains(ip));
        }

        #endregion

        /// <summary>
        /// Writes the specified content wrapped in a DIV tag.
        /// </summary>
        public static void WriteLine(this HttpResponse response, string content) =>
            response.WriteAsync($"<div>{content}</div>").RunSynchronously();

        /// <summary>
        /// Redirects the client to the specified URL with a 301 status (permanent).
        /// </summary>
        public static void RedirectPermanent(this HttpResponse response, string permanentUrl)
        {
            response.StatusCode = MOVED_PERMANENTLY_STATUS_CODE;
            response.Headers.Add("Location", permanentUrl);
        }

        /// <summary>
        /// Removes the specified query string parameter.
        /// </summary>
        public static Uri RemoveEmptyQueryParameters(this Uri url)
        {
            var toRemove = url.GetQueryString().Where(x => x.Value.IsEmpty()).ToList();

            foreach (var item in toRemove) url = url.RemoveQueryString(item.Key);

            return url;
        }

        /// <summary>
        /// Removes the specified query string parameter.
        /// </summary>
        public static Uri RemoveQueryString(this Uri url, string key)
        {
            var qs = url.GetQueryString();
            key = key.ToLower();
            if (qs.ContainsKey(key)) qs.Remove(key);

            return url.ReplaceQueryString(qs);
        }

        /// <summary>
        /// Removes all query string parameters of this Url and instead adds the specified ones.
        /// </summary>
        public static Uri ReplaceQueryString(this Uri baseUrl, Dictionary<string, string> queryStringDictionary)
        {
            var r = new StringBuilder();

            r.Append(baseUrl.Scheme);

            r.Append("://");

            r.Append(baseUrl.Host);

            if (baseUrl.Port != HTTP_PORT_NUMBER && baseUrl.Port != HTTPS_PORT_NUMBER) r.Append(":" + baseUrl.Port);

            r.Append(baseUrl.AbsolutePath);

            var query = queryStringDictionary.Select(a => "{0}={1}".FormatWith(a.Key, a.Value.UrlEncode())).ToString("&");

            if (query.HasValue())
            {
                r.Append("?");
                r.Append(query);
            }

            return new Uri(r.ToString());
        }

        /// <summary>
        /// Adds the specified list to session state and returns a unique Key for that.
        /// </summary>
        public static string AddList<T>(this ISession session, IEnumerable<T> list) where T : IEntity =>
            AddList<T>(session, list, TimeSpan.FromHours(1));

        /// <summary>
        /// Adds the specified list to session state and returns a unique Key for that.
        /// </summary>
        public static string AddList<T>(this ISession session, IEnumerable<T> list, TimeSpan timeout) where T : IEntity
        {
            var expiryDate = DateTime.Now.Add(timeout);

            var key = "L|" + Guid.NewGuid() + "|" + expiryDate.ToOADate();
            session.SetString(key, list.Where(x => x != null).Select(a => a.GetId()).ToString("|").Or(string.Empty));

            var expiredKeys = session.Keys.Where(k => k.StartsWith("L|") && k.Split('|').Length == 3 && DateTime.FromOADate(k.Split('|').Last().To<double>()) < DateTime.Now).ToArray();
            expiredKeys.Do(k => session.Remove(k));

            return key;
        }

        /// <summary>
        /// Retrieves a list of objects specified by the session key which is previously generated by Session.AddList() method.
        /// </summary>
        public static async Task<IEnumerable<T>> GetList<T>(this ISession session, string key) where T : Entity
        {
            if (key.IsEmpty())
                throw new ArgumentNullException(nameof(key));

            if (key.Split('|').Length != 3)
                throw new ArgumentException("Invalid list key specified. Bar character is expected.");

            var date = key.Split('|').Last().TryParseAs<double>();

            if (date == null)
                throw new ArgumentException("Invalid list key specified. Data after Bar character should be OADate.");

            var ids = session.GetString(key);
            if (ids == null)
                throw new TimeoutException($"The list with the key {key} is expired and removed from the session.");

            return (await ids.Split('|').Select(async i => await Entity.Database.GetOrDefault<T>(i)).AwaitAll()).ExceptNull();
        }

        /// <summary>
        /// Runs the parallel select in the current HTTP context.
        /// </summary>
        public static ParallelQuery<TResult> SelectInHttpContext<TSource, TResult>(this ParallelQuery<TSource> list, Func<TSource, TResult> selector)
        {
            var httpContext = Context.HttpContextAccessor.HttpContext;

            return list.Select(x => { Context.HttpContextAccessor.HttpContext = httpContext; return selector(x); });
        }

        /// <summary>
        /// Determines if the specified argument exists in the request (form, query string or route).
        /// </summary>
        public static bool Has(this HttpRequest request, string argument)
        {
            if (request.HasFormContentType && request.Form.ContainsKey(argument)) return true;
            else if (request.Query.ContainsKey(argument)) return true;
            else return request.GetRouteValues()[argument].ToStringOrEmpty().HasValue();
        }

        public static RouteValueDictionary GetRouteValues(this HttpRequest request)
        {
            return Context.ActionContextAccessor.ActionContext.RouteData.Values;
        }

        /// <summary>
        /// Determines if the specified argument not exists in the request (query string or form).
        /// </summary>
        public static bool Lacks(this HttpRequest request, string argument) => !request.Has(argument);

        /// <summary>
        /// Gets the root of the requested website.
        /// </summary>
        public static string GetWebsiteRoot(this HttpRequest request) => $"{request.Scheme}://{request.Host}/";

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
            $"{request.GetWebsiteRoot().TrimEnd('/')}{request.PathBase}{request.Path}{request.QueryString}";

        /// <summary>
        /// Gets the absolute URL for a specified relative url.
        /// </summary>
        public static string GetAbsoluteUrl(this HttpRequest request, string relativeUrl) =>
            request.GetWebsiteRoot() + relativeUrl.TrimStart("/");

        /// <summary>
        /// Downloads the text in this URL.
        /// </summary>
        public static async Task<string> Download(this Uri url, string cookieValue = null, int timeOut = DEFAULT_DOWNLOAD_TIMEOUT)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Timeout = timeOut;

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(url, cookieValue.OrEmpty());
            }

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                    return await stream.ReadAllText();
            }
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static async Task<byte[]> DownloadData(this Uri url, string cookieValue = null, int timeOut = DEFAULT_DOWNLOAD_TIMEOUT)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            request.Timeout = timeOut;

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(url, cookieValue.OrEmpty());
            }

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                    return await stream.ReadAllBytes();
            }
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static async Task<Blob> DownloadBlob(this Uri url, string cookieValue = null, int timeOut = DEFAULT_DOWNLOAD_TIMEOUT)
        {
            var fileName = "File.Unknown";

            if (url.IsFile)
                fileName = url.ToString().Split('/').Last();

            return new Blob(await url.DownloadData(cookieValue, timeOut), fileName);
        }

        /// <summary>
        /// Reads all text in this stream as UTF8.
        /// </summary>
        public static async Task<string> ReadAllText(this Stream response)
        {
            string result = "";

            // Pipes the stream to a higher level stream reader with the required encoding format.
            using (var readStream = new StreamReader(response, Encoding.UTF8))
            {
                var read = new char[256];
                // Reads 256 characters at a time.
                int count = await readStream.ReadAsync(read, 0, read.Length);

                while (count > 0)
                {
                    // Dumps the 256 characters on a string and displays the string to the console.
                    result += new string(read, 0, count);

                    count = await readStream.ReadAsync(read, 0, read.Length);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the Html Encoded version of this text.
        /// </summary>
        public static string HtmlEncode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.HtmlEncode(text);
        }

        /// <summary>
        /// Gets the Html Decoded version of this text.
        /// </summary>
        public static string HtmlDecode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.HtmlDecode(text);
        }

        /// <summary>
        /// Gets the Url Encoded version of this text.
        /// </summary>
        public static string UrlEncode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.UrlEncode(text);
        }

        /// <summary>
        /// Gets the Url Decoded version of this text.
        /// </summary>
        public static string UrlDecode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.UrlDecode(text);
        }

        /// <summary>
        /// Properly sets a query string key value in this Uri, returning a new Uri object.
        /// </summary>
        public static Uri SetQueryString(this Uri uri, string key, object value)
        {
            var valueString = string.Empty;

            if (value != null)
            {
                if (value is IEntity)
                    valueString = (value as IEntity).GetId().ToString();
                else
                    valueString = value.ToString();
            }

            var pairs = HttpUtility.ParseQueryString(uri.Query);

            pairs[key] = valueString;

            var builder = new UriBuilder(uri) { Query = pairs.ToString() };

            return builder.Uri;
        }
    }
}