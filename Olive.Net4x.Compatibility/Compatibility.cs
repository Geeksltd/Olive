using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Security;

namespace Olive
{
    /// <summary>
    /// Prepares a legacy .NET 4.x application to use Olive.
    /// </summary>
    public class Compatibility
    {
        /// <summary>
        /// Initializes Olive context for legacy (non ASP.NET Core apps).
        /// </summary>
        public static void Initialize()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var services = new ServiceCollection();
            services.AddLogging();
            //services.AddSingleton(typeof(ILoggerFactory), new LoggerFactory());
            services.AddSingleton(typeof(IConfiguration), new XmlConfigReader());
            services.AddSingleton(typeof(IHttpClientFactory), new HttpClientFactory());

            var provider = new BasicOliveServiceProvider(services);
            Context.Initialize(provider, () => provider);
        }

        public class HttpClientFactory : IHttpClientFactory
        {
            Dictionary<string, HttpClient> All = new Dictionary<string, HttpClient>();
            public HttpClient CreateClient(string name) => All.TryGet(name) ?? (All[name] = new HttpClient());
        }

        /// <summary>
        /// To be invoked when loading production runtime secrets.
        /// </summary>
        public static void LoadSecrets(IDictionary<string, string> secrets)
        {
            foreach (var item in secrets)
            {
                if (item.Key.StartsWith("ConnectionStrings"))
                {
                    ConfigurationManager.ConnectionStrings[item.Key.Split(':')[1]]
                        .ConnectionString = item.Value;
                }
                else
                {
                    ConfigurationManager.AppSettings[item.Key] = item.Value;
                }
            }
        }
    }
}