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

        public Startup(IConfiguration config)
        {
            Configuration = config;
            var context = Context.Current;
            ConfigureServices(context.Services);
            Context.Current.Set(context.Services.BuildServiceProvider());
            Environment = context.GetService<IHostingEnvironment>();

            Log.Init(context.GetService<ILoggerFactory>());
        }

        public virtual void ConfigureServices(IServiceCollection services) { }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            await Run();
            Process.GetCurrentProcess().Kill();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected abstract Task Run();
    }
}
