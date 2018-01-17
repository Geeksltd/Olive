using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Olive.Entities;

namespace Olive.Web
{
    partial class OliveExtensions
    {
        const int HTTP_PORT_NUMBER = 80;
        const int HTTPS_PORT_NUMBER = 443;

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