using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Olive.Console
{
    /// <summary>
    /// A host container for console apps using Olive services.
    /// </summary>
    public static class Application
    {
        public static Task Start<TStartup>(string[] args, Action<HostBuilder> configure = null) where TStartup : Startup
        {
            Startup.Args = args;

            var host = new HostBuilder();
            configure?.Invoke(host);

            return host.ConfigureServices((hostContext, services) =>
             {
                 Context.Initialize(services);
                 services.AddHostedService<TStartup>();
             })
                .RunConsoleAsync();
        }
    }
}