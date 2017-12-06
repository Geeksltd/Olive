using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Web
{
    public static class UrlRewriting
    {
        static Dictionary<Type, Func<IWebResource, string>> RewritingMapping = new Dictionary<Type, Func<IWebResource, string>>();
        static Dictionary<Type, Func<string, IWebResource>> ResourceLoaders = new Dictionary<Type, Func<string, IWebResource>>();

        /// <summary>
        /// Creates a suitable string for urls.
        /// </summary>        
        public static string Escape(string text)
        {
            if (text.IsEmpty()) return string.Empty;

            var r = new StringBuilder();

            r.Append(string.Empty);

            foreach (var c in text)
            {
                if (c.IsLetterOrDigit()) r.Append(c);
                switch (c)
                {
                    case '-':
                    case '_': r.Append(c); break;
                    case ' ': r.Append('-'); break;
                    case '&': r.Append("-and-"); break;
                    default: break; // Keep it
                }
            }

            var result = r.ToString().Trim("-_".ToCharArray());

            while (result.Contains("__"))
                result = result.Replace("__", "_");

            while (result.Contains("--"))
                result = result.Replace("--", "-");

            return result;
        }

        public static void RegisterLoader<T>(Func<string, IWebResource> loaderMethod) => ResourceLoaders.Add(typeof(T), loaderMethod);

        static Func<IWebResource, string> GetMapper(Type type)
        {
            for (var parent = type; parent != null; parent = parent.BaseType)
                if (RewritingMapping.ContainsKey(type))
                    return RewritingMapping[type];

            throw new Exception("No Url Rewrite mapping has been specified for the type " + type.FullName);
        }

        public static void Register<T>(Func<T, string> mapping) where T : IWebResource
        {
            if (!RewritingMapping.ContainsKey(typeof(T)))
                RewritingMapping.Add(typeof(T), r => mapping?.Invoke((T)r));
        }

        public static string GetExecutionPath(IWebResource resource)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));

            var path = GetMapper(resource.GetType())(resource);

            var queryString = Context.HttpContextAccessor.HttpContext.Request.Query;
            if (queryString.Keys.Any())
            {
                foreach (var key in queryString.Keys)
                {
                    if (path.Contains(key + "=")) continue;

                    if (!path.Contains("?")) path += "?";
                    else if (!path.EndsWith("&")) path += "&";
                    path += key + "=" + queryString[key];
                }
            }

            return path;
        }

        /// <summary>
        /// Gets the Currently requested resource.
        /// </summary>
        public static Task<IWebResource> FindRequestedResource() => FindRequestedResource(ignoreDomain: true);

        /// <summary>
        /// Gets the Currently requested resource.
        /// </summary>
        public static async Task<IWebResource> FindRequestedResource(bool ignoreDomain)
        {
            var path = Context.HttpContextAccessor.HttpContext.Request.Path.ToString();

            if (!ignoreDomain)
            {
                if (path.StartsWith("/")) path = path.Substring(1);
                path = Context.HttpContextAccessor.HttpContext.Request.GetWebsiteRoot() + path;
            }

            IWebResource result;

            foreach (var type in RewritingMapping.Keys)
            {
                if (ResourceLoaders.ContainsKey(type))
                    result = ResourceLoaders[type](path);
                else
                    result = (await Entity.Database.Of(type).GetList()).OfType<IWebResource>().FirstOrDefault(r => r.GetUrl().Equals(path, StringComparison.OrdinalIgnoreCase));

                if (result != null) return result;
            }

            return null;
        }

        /// <summary>
        /// Determines if this web resource's Url matches a given path.
        /// </summary>
        public static bool Matches(this IWebResource resource, string path) =>
            resource.GetUrl().Equals(path, StringComparison.OrdinalIgnoreCase);
    }
}