using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;

namespace Olive
{
    /// <summary> 
    /// Provides shortcut access to the value specified in web.config (or App.config) under AppSettings or ConnectionStrings.
    /// </summary>
    public static class Config
    {
        const string CONNECTION_STRINGS_CONFIG_ROOT = "ConnectionStrings";

        static IConfiguration Configuration => Context.Current.GetService<IConfiguration>();

        ///// <summary>
        ///// Gets the connection string with the specified key.
        ///// <para>The connection strings should store directly under the ConnectionStrings section.</para>
        ///// </summary>
        //public static string GetConnectionString(string key) => GetOrThrow($"ConnectionStrings:{key}");

        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <param name="instance">The object to bind.</param>
        public static void Bind(string key, object instance) => GetSection(key).Bind(instance);

        /// <summary>
        /// Attempts to bind a new instance of given type to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <returns>A new instance of the given generic type.</returns>
        public static T Bind<T>(string key) where T : new()
        {
            var result = new T();
            GetSection(key).Bind(result);
            return result;
        }

        public static IConfigurationSection GetSection(string key)
            => Configuration.GetSection(key);

        /// <summary>
        /// Gets the requested configuration value, or empty string if none is provided.
        /// </summary>
        public static string Get(string key) => Get(key, string.Empty);

        public static string GetOrThrow(string key)
        {
            var result = Get(key);

            if (result.HasValue()) return result;
            else throw new Exception($"AppSetting value of '{key}' is not specified.");
        }

        public static string Get(string key, string defaultValue)
            => Configuration.GetValue(key, defaultValue);

        public static T Get<T>(string key) => Get<T>(key, default(T));

        public static T Get<T>(string key, T defaultValue) => Configuration.GetValue(key, defaultValue);

        public static Dictionary<string, string> GetSubsection(string sectionKey, bool relativeKeys)
        {
            var settings = GetSection(sectionKey)?.AsEnumerable(relativeKeys) ??
                Enumerable.Empty<KeyValuePair<string, string>>();

            return settings.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// In all config values, replaces %SOMETHING% with the current environment variable with the same name.
        /// If no environment variable with that name exists, it does not replace it.
        /// </summary>
        public static IConfiguration MergeEnvironmentVariables(this IConfiguration config)
        {
            var keys = Environment.GetEnvironmentVariables().Keys.Cast<string>().ToArray();
            foreach (var variable in keys)
            {
                var key = $"%{variable}%";
                var configNodes = config.AsEnumerable().Where(v => v.Value.OrEmpty().Contains(key)).ToArray();
                foreach (var item in configNodes)
                {
                    var value = Environment.GetEnvironmentVariable(variable);
                    var finalValue = item.Value.Replace(key, value);
                    try
                    {
                        config[item.Key] = finalValue;
                    }
                    catch
                    {
                        Console.WriteLine("Failed to update config key from environment variable.");
                    }
                }
            }

            return config;
        }
    }
}