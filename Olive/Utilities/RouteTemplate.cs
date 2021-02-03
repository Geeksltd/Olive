using System;
using System.Collections.Generic;
using System.Linq;

namespace Olive
{
    /// <summary>
    /// Provides processing features for url route patterns.
    /// </summary>
    public class RouteTemplate
    {
        public string Template;
        public List<RouteTemplateParameter> Parameters = new List<RouteTemplateParameter>();
        public RouteTemplate(string pattern)
        {
            Template = pattern;
            var remaining = pattern;
            while (remaining.Contains("{"))
            {
                var parameter = remaining.Substring("{", "}", inclusive: true);
                var key = parameter.TrimStart("{").TrimEnd("}");
                var mandatory = !key.EndsWith("?");
                key = key.TrimEnd("?");
                var type = typeof(string);
                if (key.Contains(":"))
                {
                    // TODO: Type doesn't matter in this use case.
                    // type = Type.GetType(key.Split(':').Last());
                    key = key.Split(':').First();
                }

                Parameters.Add(new RouteTemplateParameter { Parameter = parameter, Key = key.ToCamelCaseId(), Type = type, IsMandatory = mandatory });
                remaining = remaining.Substring(remaining.IndexOf("{") + 1);
            }
        }

        public bool IsMatch(Dictionary<string, string> routeData)
        {
            foreach (var p in Parameters)
            {
                var routeKey = routeData.Keys.FirstOrDefault(x => x.ToUpper() == p.Key.ToUpper());
                if (routeKey != null)
                {
                    if (!p.MatchesType(routeData[routeKey]))
                        return false;
                }
                else if (p.IsMandatory)
                {
                    // The value of this parameter in the route is not provided.
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<RouteTemplateParameter> FindMatchingParameters(Dictionary<string, string> routeData)
        {
            foreach (var p in Parameters)
            {
                var routeKey = routeData.Keys.FirstOrDefault(x => x.ToUpper() == p.Key.ToUpper());
                if (routeKey is null) continue;
                if (!p.MatchesType(routeData[routeKey])) continue;
                yield return p;
            }
        }

        public override string ToString() => Template;

        /// <summary>
        /// It will merge the provided route data parameters into the pattern of the template.
        /// If any parameter in the template is non-optional, and yet a value has not been provided, it will throw an error.
        /// If any of the provided route data parameters aren't expected in the pattern, then they will be added to the query string.
        /// </summary>
        public string Merge(object routeData) => Merge(new Dictionary<string, string>().AddFromProperties(routeData));

        /// <summary>
        /// It will merge the provided route data parameters into the pattern of the template.
        /// If any parameter in the template is non-optional, and yet a value has not been provided, it will throw an error.
        /// If any of the provided route data parameters aren't expected in the pattern, then they will be added to the query string.
        /// </summary>
        public string Merge(Dictionary<string, string> routeData)
        {
            if (routeData == null)
                throw new ArgumentNullException(nameof(routeData));
            var result = Template;
            foreach (var p in Parameters)
            {
                var routeKey = routeData.Keys.FirstOrDefault(x => x.ToUpper() == p.Key.ToUpper());
                if (routeKey != null)
                {
                    // TODO: Type check
                    result = result.Replace(p.Parameter, routeData[routeKey]);
                    routeData.Remove(routeKey);
                }
                else
                {
                    if (p.IsMandatory)
                    {
                        throw new Exception("The value of " + p.Parameter + " in the route " + Template + " is not provided.");
                    }

                    result = result.Remove(p.Parameter);
                }
            }

            result = "/" + result.KeepReplacing("//", "/").TrimEnd("/").TrimStart("/");
            result += routeData.Select(x => x.Key + "=" + x.Value.UrlEncode()).ToString("&").WithPrefix("?");
            return result;
        }

        public class RouteTemplateParameter
        {
            public string Parameter; // Example: {key:type?}
            public string Key;
            public Type Type;
            public bool IsMandatory;

            internal bool MatchesType(string value)
            {
                try
                {
                    value.To(Type);
                    // No error?
                    return true;
                }
                catch
                {
                    // No logging is needed
                    return false;
                }
            }
        }
    }
}