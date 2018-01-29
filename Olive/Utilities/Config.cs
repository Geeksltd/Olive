using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Olive
{
    /// <summary> 
    /// Provides shortcut access to the value specified in web.config (or App.config) under AppSettings or ConnectionStrings.
    /// </summary>
    public static class Config
    {
        static IConfiguration Configuration;

        const string CONNECTION_STRINGS_CONFIG_ROOT = "ConnectionStrings";

        static Config()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the connection string with the specified key.
        /// <para>The connection strings should store directly under the ConnectionStrings section.</para>
        /// </summary>
        public static string GetConnectionString(string key)
        {
            return Configuration[$"{CONNECTION_STRINGS_CONFIG_ROOT}:{key}"] ??
                throw new ArgumentException($"Thre is no connectionString defined in the appsettings.json or web.config with the key '{key}'.");
        }

        /// <summary>
        /// Gets all the connection strings.
        /// <para>The connection strings should store directly under the ConnectionStrings section.</para>
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetConnectionStrings()
        {
            return SettingsUnder(CONNECTION_STRINGS_CONFIG_ROOT);
        }

        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <param name="instance">The object to bind.</param>
        public static void Bind(string key, object instance) => Section(key).Bind(instance);

        /// <summary>
        /// Attempts to bind a new instance of given type to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <returns>A new instance of the given generic type.</returns>
        public static T Bind<T>(string key) where T : new()
        {
            var result = new T();
            Section(key).Bind(result);
            return result;
        }

        public static IConfigurationSection Section(string key) => Configuration.GetSection(key);

        /// <summary>
        /// Gets the child nodes under a specified parent key.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> SettingsUnder(string key)
        {
            return Section(key)
                   .GetChildren()
                   .Select(section => new KeyValuePair<string, string>(section.Key, section.Value));
        }

        /// <summary>
        /// Gets the value configured in Web.Config (or App.config) under AppSettings.
        /// </summary>
        public static string Get(string key) => Get(key, string.Empty);

        public static string GetOrThrow(string key)
        {
            var result = Get(key);
            if (result.IsEmpty())
                throw new Exception($"AppSetting value of '{key}' is not specified.");

            return result;
        }

        /// <summary>
        /// Gets the value configured in Web.Config (or App.config) under AppSettings.
        /// If no value is found there, it will return the specified default value.
        /// </summary>
        public static string Get(string key, string defaultValue) => Configuration[key].Or(defaultValue);

        /// <summary>
        /// Reads the value configured in Web.Config (or App.config) under AppSettings.
        /// It will then convert it into the specified type.
        /// </summary>
        public static T Get<T>(string key) => Get<T>(key, default(T));

        /// <summary>
        /// Reads the value configured in Web.Config (or App.config) under AppSettings.
        /// It will then convert it into the specified type.
        /// If no value is found there, it will return the specified default value.
        /// </summary>
        public static T Get<T>(string key, T defaultValue)
        {
            var value = "[???]";
            try
            {
                value = Get(key, defaultValue.ToStringOrEmpty());

                if (value.IsEmpty()) return defaultValue;

                var type = typeof(T);

                return value.To<T>();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not retrieve '{0}' config value for key '{1}' and value '{2}'.".FormatWith(typeof(T).FullName, key, value), ex);
            }
        }

        /// <summary>
        /// Reads the value configured in Web.Config (or App.config) under AppSettings.
        /// It will then try to convert it into the specified type.
        /// If no vale is found in AppSettings or the conversion fails, then it will return null, or the default value of the specified type T.
        /// </summary>
        public static T TryGet<T>(string key)
        {
            var value = Get(key);

            if (value.IsEmpty()) return default(T);

            var type = typeof(T);

            try
            {
                return (T)value.To(type);
            }
            catch
            {
                // No logging is needed
                return default(T);
            }
        }

        /// <summary>
        /// Determines whether the specified key is defined in configuration file.
        /// </summary>
        public static bool IsDefined(string key) => Get(key).HasValue();

        /// <summary>
        /// Reads the app settings from a specified configuration file.
        /// </summary>
		[Obsolete("The XML settings are outdated.")]
        public static async Task<Dictionary<string, string>> ReadAppSettingsAsync(FileInfo configFile)
        {
            if (configFile == null) throw new ArgumentNullException(nameof(configFile));

            if (!configFile.Exists()) throw new ArgumentException("File does not exist: " + configFile.FullName);

            var result = new Dictionary<string, string>();

            var config = XDocument.Parse(await configFile.ReadAllTextAsync());

            var appSettings = config.Root.Elements().SingleOrDefault(a => a.Name.LocalName == "appSettings");

            foreach (var setting in appSettings?.Elements())
            {
                var key = setting.GetValue<string>("@key");

                if (result.ContainsKey(key))
                    throw new Exception($"The key '{key}' is defined more than once in the application config file '{configFile.FullName}'.");

                result.Add(key, setting.GetValue<string>("@value"));
            }

            return result;
        }
    }
}