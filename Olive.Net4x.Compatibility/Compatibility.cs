using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

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
            Context.Initialize(services);
            Context.Current.Set(new BasicOliveServiceProvider(services));

            Context.Current.AddService(typeof(IConfiguration), new XmlConfigReader());
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