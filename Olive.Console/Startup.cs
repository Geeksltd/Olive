using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Olive.Console
{
    public abstract class Startup : IHostedService
    {
        public static string[] Args;

        public static IHostingEnvironment Environment { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration config, IServiceProvider provider)
        {
            Configuration = config;
            Context.Current.Set(provider);
            Environment = Context.Current.GetService<IHostingEnvironment>();

            var logger = Context.Current.GetService<ILoggerFactory>();
            Log.Init(logger);
        }

        public virtual void ConfigureServices(IServiceCollection services) { }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            ConfigureServices(Context.Current.Services);
            await Run();

            Process.GetCurrentProcess().Kill();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected abstract Task Run();
    }
}
