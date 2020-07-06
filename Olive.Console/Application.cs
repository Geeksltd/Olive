using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Olive.Console
{
    /// <summary>
    /// A host container for console apps using Olive services.
    /// </summary>
    public static class Application
    {
        internal static IServiceCollection ServiceCollection;

        public static Task Start<TStartup>(string[] args, Action<HostBuilder> configure = null) where TStartup : Startup
        {
            Startup.Args = args;

            var host = new HostBuilder();
            host.ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config.AddJsonFile("appsettings.json", optional: true);
                 config.AddEnvironmentVariables();
                 if (args != null) config.AddCommandLine(args);
             });

            configure?.Invoke(host);

            host.ConfigureServices((hostContext, services) =>
            {
                ServiceCollection = services;
                services.AddOptions();
                services.AddHostedService<TStartup>();
            });

            host.ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            return host.RunConsoleAsync();
        }
    }
}