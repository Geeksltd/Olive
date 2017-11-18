using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Olive
{
    /// <summary>
    /// Provides shortcut access to the value specified in web.config (or App.config) under AppSettings or ConnectionStrings.
    /// </summary>
    public static class Config
    {
        //static IConfiguration Configuration;
        static AppSettings Configuration;

        const string CONNECTION_STRINGS_CONFIG_ROOT = "ConnectionStrings";

        static Config()
        {
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            //Configuration = builder.Build();
            try
            {
                var converter = new PessimisticJsonConverter();
                Configuration = converter.ReadJson(File.ReadAllText(Directory.GetCurrentDirectory() + "/appsettings.json"), typeof(AppSettings)) as AppSettings;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }

        /// <summary>
        /// Gets the connection string with the specified key.
        /// <para>The connection strings should store directly under the ConnectionStrings section.</para>
        /// </summary>
        public static string GetConnectionString(string key)
        {
            //return Configuration[$"{CONNECTION_STRINGS_CONFIG_ROOT}:{key}"] ??
            //    throw new ArgumentException($"Thre is no connectionString defined in the appsettings.json or web.config with the key '{key}'.");
            return Configuration.ConnectionStrings.AppDatabase ??
                throw new ArgumentException($"Thre is no connectionString defined in the appsettings.json or web.config with the key '{key}'.");
        }

        /// <summary>
        /// Gets all the connection strings.
        /// <para>The connection strings should store directly under the ConnectionStrings section.</para>
        /// </summary>
        public static ConnectionStrings GetConnectionStrings()
        {
            //return Configuration.GetSection(CONNECTION_STRINGS_CONFIG_ROOT).GetChildren()
            //    .Select(section => new KeyValuePair<string, string>(section.Key, section.Value));
            return Configuration.ConnectionStrings;
        }

        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <param name="instance">The object to bind.</param>
        public static void Bind(string key, object instance)
        {
            //Configuration.GetSection(key).Bind(instance);
        }

        /// <summary>
        /// Attempts to bind a new instance of given type to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <returns>A new instance of the given generic type.</returns>
        public static dynamic Bind<T>(string key) where T : new()
        {
            switch (key)
            {
                case "DataProviderModel":
                    return Configuration.DataProviderModel;
                default:
                    return new object();
            }
            //Configuration.GetSection(key).Bind(result);
        }

        /// <summary>
        /// Gets the value configured in Web.Config (or App.config) under AppSettings.
        /// </summary>
        public static string Get(string key) => Get(key, string.Empty);

        /// <summary>
        /// Gets the value configured in Web.Config (or App.config) under AppSettings.
        /// If no value is found there, it will return the specified default value.
        /// </summary>
        public static string Get(string key, string defaultValue)
        {
            //return Configuration[key].Or(defaultValue);
            switch (key)
            {
                case "App.Resource.Version":
                    return Configuration.AppResourceVersion;
                case "Authentication:Timeout":
                    return Configuration.Authentication.Timeout.ToString();
                case "Authentication:Provider":
                    return Configuration.Authentication.Provider;
                case "Authentication:LoginUrl":
                    return Configuration.Authentication.LoginUrl;
                case "Default.TransactionScope.Type":
                    return Configuration.DefaultTransactionScopeType;
                case "Default.Transaction.IsolationLevel":
                    return Configuration.DefaultTransactionIsolationLevel;
                case "Webpages:Version":
                    return Configuration.Webpages.Version;
                case "Webpages:Enabled":
                    return Configuration.Webpages.Enabled.ToString();
                case "ClientValidationEnabled":
                    return Configuration.ClientValidationEnabled.ToString();
                case "UnobtrusiveJavaScriptEnabled":
                    return Configuration.UnobtrusiveJavaScriptEnabled.ToString();
                case "Secure.Password.Pbkdf2.Iterations":
                    return Configuration.SecurePasswordPbkdf2Iterations.ToString();
                case "Database:Cache.Enabled":
                    return Configuration.Database.CacheEnabled.ToString();
                case "Database:Save.Enforce.Transaction":
                    return Configuration.Database.SaveEnforceTransaction.ToString();
                case "Database:Storage.Path":
                    return Configuration.Database.StoragePath;
                case "Database:Concurrency.Aware.Cache":
                    return Configuration.Database.ConcurrencyAwareCache.ToString();
                case "Temp.Databases.Location":
                    return Configuration.TempDatabasesLocation;
                case "Test.Files.Origin:Open":
                    return Configuration.TestFilesOrigin.Open;
                case "Test.Files.Origin:Secure":
                    return Configuration.TestFilesOrigin.Secure;
                case "UploadFolder":
                    return Configuration.UploadFolder;
                case "UploadFolder.VirtualRoot":
                    return Configuration.UploadFolderVirtualRoot;
                case "Translate.Validators":
                    return Configuration.TranslateValidators.ToString();
                case "Email:Enable.Ssl":
                    return Configuration.Email.EnableSsl.ToString();
                case "Email:Sender:Address":
                    return Configuration.Email.Sender.Address;
                case "Email:Sender:Name":
                    return Configuration.Email.Sender.Name;
                case "Email:Permitted.Domains":
                    return Configuration.Email.PermittedDomains;
                case "Email:Maximum.Retries":
                    return Configuration.Email.MaximumRetries.ToString();
                case "Email:Auto.CC.Address":
                    return Configuration.Email.AutoCCAddress;
                case "Log.Record:Exceptions":
                    return Configuration.LogRecord.Exceptions.ToString();
                case "Log.Record:Application:Events":
                    return Configuration.LogRecord.Application.Events.ToString();
                case "Log.Record:Application:Events.SkipInsertData":
                    return Configuration.LogRecord.Application.EventsSkipInsertData.ToString();
                case "Facebook:AppID":
                    return Configuration.Facebook.AppID;
                case "Facebook:AppSecret":
                    return Configuration.Facebook.AppSecret;
                case "Google:ClientId":
                    return Configuration.Google.ClientId;
                case "Google:ClientSecret":
                    return Configuration.Google.ClientSecret;
                case "Error.Notification.Receiver":
                    return Configuration.ErrorNotificationReceiver;
                case "Data.Access.Log.Custom.Queries":
                    return Configuration.DataAccessLogCustomQueries.ToString();
                case "GZip.Pages.Response":
                    return Configuration.GZipPagesResponse.ToString();
                case "Pages.CommonResources.Enabled":
                    return Configuration.PagesCommonResourcesEnabled.ToString();
                case "Automated.Tasks:Enabled":
                    return Configuration.AutomatedTasks.Enabled.ToString();
                case "Automated.Tasks:Persist.Execution":
                    return Configuration.AutomatedTasks.PersistExecution.ToString();
                case "Automated.Tasks:Status.Path":
                    return Configuration.AutomatedTasks.StatusPath;
                case "Logging:IncludeScopes":
                    return Configuration.Logging.IncludeScopes.ToString();
                case "Logging:LogLevel:Default":
                    return Configuration.Logging.LogLevel.Default;
                default: return defaultValue;
            }
        }

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
        public static async Task<Dictionary<string, string>> ReadAppSettings(FileInfo configFile)
        {
            if (configFile == null) throw new ArgumentNullException(nameof(configFile));

            if (!configFile.Exists()) throw new ArgumentException("File does not exist: " + configFile.FullName);

            var result = new Dictionary<string, string>();

            var config = XDocument.Parse(await configFile.ReadAllText());

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

    #region appsettings.json model
    [DataContract(Name = "ConnectionStrings")]
    public class ConnectionStrings
    {
        [DataMember(Name = "AppDatabase")]
        public string AppDatabase { get; set; }
    }

    [DataContract(Name = "Provider")]
    public class DataProviderFactoryInfo
    {
        string mappingDirectory;

        [DataMember(Name = "AssemblyName")]
        public string AssemblyName { get; set; }

        [DataMember(Name = "ProviderFactoryType")]
        public string ProviderFactoryType { get; set; }

        public string MappingResource { get; set; }

        public string TypeName { get; set; }

        public string MappingDirectory
        {
            get => mappingDirectory;
            set
            {
                if (value == null)
                    mappingDirectory = string.Empty;

                else if (value.StartsWith("\\\\") || value.Contains(":"))
                    // Absolute path:
                    mappingDirectory = value;

                else
                {
                    mappingDirectory = AppDomain.CurrentDomain.BaseDirectory + "/" + value + "/";
                    mappingDirectory = mappingDirectory.Replace("/", "\\");

                    mappingDirectory = mappingDirectory.KeepReplacing(@"\\", @"\");
                }
            }
        }

        public string ConnectionStringKey { get; set; }

        public string ConnectionString { get; set; }

        public Assembly Assembly { get; set; }
        public Type Type { get; set; }
    }

    [DataContract(Name = "DataProviderModel")]
    public class DataProviderModelConfigurationSection
    {
        [DataMember(Name = "Providers")]
        public virtual List<DataProviderFactoryInfo> Providers { get; set; }

        public virtual string SyncFilePath { get; set; }

        public virtual string FileDependancyPath { get; set; }
    }

    [DataContract(Name = "Authentication")]
    public class Authentication
    {
        [DataMember(Name = "Timeout")]
        public int Timeout { get; set; }

        [DataMember(Name = "Provider")]
        public string Provider { get; set; }

        [DataMember(Name = "LoginUrl")]
        public string LoginUrl { get; set; }
    }

    [DataContract(Name = "Webpages")]
    public class Webpages
    {
        [DataMember(Name = "Version")]
        public string Version { get; set; }

        [DataMember(Name = "Enabled")]
        public bool Enabled { get; set; }
    }

    [DataContract(Name = "Database")]
    public class DatabaseSetting
    {
        [DataMember(Name = "Cache.Enabled")]
        public bool CacheEnabled { get; set; }

        [DataMember(Name = "Save.Enforce.Transaction")]
        public bool SaveEnforceTransaction { get; set; }

        [DataMember(Name = "Storage.Path")]
        public string StoragePath { get; set; }

        [DataMember(Name = "Concurrency.Aware.Cache")]
        public bool ConcurrencyAwareCache { get; set; }
    }

    [DataContract(Name = "TestFilesOrigin")]
    public class TestFilesOrigin
    {
        [DataMember(Name = "Open")]
        public string Open { get; set; }

        [DataMember(Name = "Secure")]
        public string Secure { get; set; }
    }

    [DataContract(Name = "Sender")]
    public class Sender
    {
        [DataMember(Name = "Address")]
        public string Address { get; set; }

        [DataMember(Name = "Name")]
        public string Name { get; set; }
    }

    [DataContract(Name = "Email")]
    public class Email
    {
        [DataMember(Name = "Enable.Ssl")]
        public bool EnableSsl { get; set; }

        [DataMember(Name = "Sender")]
        public Sender Sender { get; set; }

        [DataMember(Name = "Permitted.Domains")]
        public string PermittedDomains { get; set; }

        [DataMember(Name = "Maximum.Retries")]
        public int MaximumRetries { get; set; }

        [DataMember(Name = "Auto.CC.Address")]
        public string AutoCCAddress { get; set; }
    }

    [DataContract(Name = "Application")]
    public class Application
    {
        [DataMember(Name = "Events")]
        public bool Events { get; set; }

        [DataMember(Name = "Events.SkipInsertData")]
        public bool EventsSkipInsertData { get; set; }
    }

    [DataContract(Name = "LogRecord")]
    public class LogRecord
    {
        [DataMember(Name = "Exceptions")]
        public bool Exceptions { get; set; }

        [DataMember(Name = "Application")]
        public Application Application { get; set; }
    }

    [DataContract(Name = "Facebook")]
    public class Facebook
    {
        [DataMember(Name = "AppID")]
        public string AppID { get; set; }

        [DataMember(Name = "AppSecret")]
        public string AppSecret { get; set; }
    }

    [DataContract(Name = "Google")]
    public class Google
    {
        [DataMember(Name = "ClientId")]
        public string ClientId { get; set; }

        [DataMember(Name = "ClientSecret")]
        public string ClientSecret { get; set; }
    }

    [DataContract(Name = "AutomatedTasks")]
    public class AutomatedTasks
    {
        [DataMember(Name = "Enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name = "Persist.Execution")]
        public bool PersistExecution { get; set; }

        [DataMember(Name = "Status.Path")]
        public string StatusPath { get; set; }
    }

    [DataContract(Name = "LogLevel")]
    public class LogLevel
    {
        [DataMember(Name = "Default")]
        public string Default { get; set; }
    }

    [DataContract(Name = "Logging")]
    public class Logging
    {
        [DataMember(Name = "IncludeScopes")]
        public bool IncludeScopes { get; set; }

        [DataMember(Name = "LogLevel")]
        public LogLevel LogLevel { get; set; }
    }

    [DataContract(Name = "AppSettings")]
    public class AppSettings
    {
        [DataMember(Name = "App.Resource.Version")]
        public string AppResourceVersion { get; set; }

        [DataMember(Name = "ConnectionStrings")]
        public ConnectionStrings ConnectionStrings { get; set; }

        [DataMember(Name = "DataProviderModel")]
        public DataProviderModelConfigurationSection DataProviderModel { get; set; }

        [DataMember(Name = "Authentication")]
        public Authentication Authentication { get; set; }

        [DataMember(Name = "Default.TransactionScope.Type")]
        public string DefaultTransactionScopeType { get; set; }

        [DataMember(Name = "Default.Transaction.IsolationLevel")]
        public string DefaultTransactionIsolationLevel { get; set; }

        [DataMember(Name = "Webpages")]
        public Webpages Webpages { get; set; }

        [DataMember(Name = "ClientValidationEnabled")]
        public bool ClientValidationEnabled { get; set; }

        [DataMember(Name = "UnobtrusiveJavaScriptEnabled")]
        public bool UnobtrusiveJavaScriptEnabled { get; set; }

        [DataMember(Name = "Secure.Password.Pbkdf2.Iterations")]
        public int SecurePasswordPbkdf2Iterations { get; set; }

        [DataMember(Name = "Database")]
        public DatabaseSetting Database { get; set; }

        [DataMember(Name = "Temp.Databases.Location")]
        public string TempDatabasesLocation { get; set; }

        [DataMember(Name = "Test.Files.Origin")]
        public TestFilesOrigin TestFilesOrigin { get; set; }

        [DataMember(Name = "UploadFolder")]
        public string UploadFolder { get; set; }

        [DataMember(Name = "UploadFolder.VirtualRoot")]
        public string UploadFolderVirtualRoot { get; set; }

        [DataMember(Name = "Translate.Validators")]
        public bool TranslateValidators { get; set; }

        [DataMember(Name = "Email")]
        public Email Email { get; set; }

        [DataMember(Name = "Log.Record")]
        public LogRecord LogRecord { get; set; }

        [DataMember(Name = "Facebook")]
        public Facebook Facebook { get; set; }

        [DataMember(Name = "Google")]
        public Google Google { get; set; }

        [DataMember(Name = "Error.Notification.Receiver")]
        public string ErrorNotificationReceiver { get; set; }

        [DataMember(Name = "Data.Access.Log.Custom.Queries")]
        public bool DataAccessLogCustomQueries { get; set; }

        [DataMember(Name = "GZip.Pages.Response")]
        public bool GZipPagesResponse { get; set; }

        [DataMember(Name = "Pages.CommonResources.Enabled")]
        public bool PagesCommonResourcesEnabled { get; set; }

        [DataMember(Name = "Automated.Tasks")]
        public AutomatedTasks AutomatedTasks { get; set; }

        [DataMember(Name = "Logging")]
        public Logging Logging { get; set; }
    }
    #endregion
}