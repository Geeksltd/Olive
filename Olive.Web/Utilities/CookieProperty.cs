namespace Olive.Web
{
    using Microsoft.AspNetCore.Http;
    using Olive.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides easy access to HTTP cookie data.
    /// </summary>
    public class CookieProperty
    {
        const string BAR_SCAPE = "[#*^BAR_SCAPE^*#]";

        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// Gets the value of the property sent from the client browser as a cookie.
        /// </summary>
        public static Task<T> Get<T>() => Get<T>(null, default(T));

        /// <summary>
        /// Gets the value of a string property sent from the client browser as a cookie.
        /// </summary>
        public static Task<string> Get(string key) => Get<string>(key, null);

        /// <summary>
        /// Gets the value of the property sent from the client browser as a cookie.
        /// </summary>
        public static Task<T> Get<T>(T defaultValue) => Get<T>(null, defaultValue);

        /// <summary>
        /// Gets the value of the property sent from the client browser as a cookie.
        /// </summary>
        public static Task<T> Get<T>(string propertyName) => Get<T>(propertyName, default(T));

        public static async Task<IEnumerable<string>> GetStrings(string propertyName) =>
            await Get<IEnumerable<string>>(propertyName, null);

        /// <summary>
        /// Gets the value of the property sent from the client browser as a cookie.
        /// </summary>
        public static async Task<T> Get<T>(string propertyName, T defaultValue)
        {
            var key = propertyName.Or("Default.Value.For." + typeof(T).FullName);

            var value = Context.Current.Request().Cookies[key];

            if (!Context.Current.Request().Cookies.ContainsKey(key))
            {
                return defaultValue;
            }
            else if (typeof(T).Implements<IEntity>())
            {
                var id = value.Contains('/') ? value.Split('/')[1] : value; // Remove class name prefix if exists
                return (T)await Database.GetOrDefault(id, typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            else if (typeof(T) == typeof(IEnumerable<string>) || typeof(T) == typeof(string[]))
            {
                return (T)(object)value.Or("").Split('|').Trim().Select(p => p.Replace(BAR_SCAPE, "|")).ToArray();
            }
            else if (typeof(T).Namespace.StartsWith("System"))
            {
                return (T)value.To(typeof(T));
            }

            throw new Exception("CookieProperty.Get<T>() does not support T type of " + typeof(T).FullName);
        }

        /// <summary>
        /// Sets a specified value in the response cookie as well as request cookie.
        /// </summary>
        /// <param name="isHttpOnly">Specifies whether the cookie should be accessible via Javascript too, or Server (http) only.</param>
        public static void Set<T>(T value, bool isHttpOnly = true) => Set<T>(null, value, isHttpOnly);

        /// <summary>
        /// Sets a specified value in the response cookie as well as request cookie.
        /// </summary>
        /// <param name="isHttpOnly">Specifies whether the cookie should be accessible via Javascript too, or Server (http) only.</param>
        public static void Set<T>(string propertyName, T value, bool isHttpOnly = true)
        {
            var key = propertyName.Or("Default.Value.For." + typeof(T).FullName);

            var stringValue = value?.ToString();

            if (value is IEntity) stringValue = (value as IEntity).GetFullIdentifierString();
            Set(key, stringValue, isHttpOnly);
        }

        /// <summary>
        /// Sets a specified list in the response cookie as well as request cookie.
        /// </summary>
        /// <param name="isHttpOnly">Specifies whether the cookie should be accessible via Javascript too, or Server (http) only.</param>
        public static void SetList<T>(string propertyName, IEnumerable<T> list, bool isHttpOnly = true) where T : IEntity
        {
            var key = propertyName.Or("Default.List.For." + typeof(T).FullName);

            if (list == null)
            {
                Set(key, string.Empty, isHttpOnly);
            }
            else
            {
                var stringValue = list.Except(n => n == null).Select(i => i.GetFullIdentifierString()).ToString("|");
                Set(key, stringValue, isHttpOnly);
            }
        }

        /// <summary>
        /// Sets a specified list in the response cookie as well as request cookie.
        /// </summary>
        public static async Task<IEnumerable<T>> GetList<T>() where T : IEntity => await GetList<T>(null);

        /// <summary>
        /// Gets a specified list in the response cookie as well as request cookie.
        /// </summary>
        public static async Task<T[]> GetList<T>(string propertyName) where T : IEntity
        {
            var key = propertyName.Or("Default.List.For." + typeof(T).FullName);

            var result = await Get(key);
            if (result.IsEmpty()) return new T[0];

            return result.Split('|').Select(x => ExtractItem<T>(x)).Except(n => n == null).ToArray();
        }

        static T ExtractItem<T>(string valueExpression) where T : IEntity
        {
            var id = valueExpression.Contains('/') ? valueExpression.Split('/')[1] : valueExpression; // Remove class name prefix if exists
            return (T)(object)Database.GetOrDefault(id, typeof(T));
        }

        /// <summary>
        /// Removes the specified cookie property.
        /// </summary>
        public static void Remove<T>() => Set<T>(default(T));

        /// <summary>
        /// Removes the specified cookie property.
        /// </summary>
        public static void Remove<T>(string propertyName) => Set<T>(propertyName, default(T));

        /// <summary>
        /// Removes the specified cookie property.
        /// </summary>
        public static void Remove(string propertyName)
        {
            var cookies = Olive.Context.Current.Response()?.Cookies;

            if (cookies == null) return;

            cookies.Delete(propertyName);
        }

        /// <summary>
        /// Sets a specified value in the response cookie as well as request cookie.
        /// </summary>
        /// <param name="isHttpOnly">Specifies whether the cookie should be accessible via Javascript too, or Server (http) only.</param>
        public static void Set(string propertyName, IEnumerable<string> strings, bool isHttpOnly = true)
        {
            strings = strings ?? new string[0];
            Set(propertyName, strings.Trim().Select(s => s.Replace("|", BAR_SCAPE)).ToString("|"), isHttpOnly);
        }

        /// <summary>
        /// Sets a specified value in the response cookie as well as request cookie.
        /// </summary>
        /// <param name="isHttpOnly">Specifies whether the cookie should be accessible via Javascript too, or Server (http) only.</param>
        public static void Set(string key, string value, bool isHttpOnly = true)
            => Set(key, value, LocalTime.Now.AddYears(10), isHttpOnly);

        /// <summary>
        /// Sets a specified value in the response cookie as well as request cookie.
        /// </summary>
        /// <param name="isHttpOnly">Specifies whether the cookie should be accessible via Javascript too, or Server (http) only.</param>
        public static void Set(string key, string value, DateTimeOffset expires, bool isHttpOnly = true)
        {
            if (key.IsEmpty())
                throw new ArgumentNullException(nameof(key));

            var cookies = Olive.Context.Current.Response()?.Cookies;

            if (cookies == null) return;

            cookies.Append(
                key,
                value,
                new CookieOptions
                {
                    HttpOnly = isHttpOnly,
                    Expires = expires,
                    Secure = Olive.Context.Current.Request().IsHttps
                }
                );
        }
    }
}