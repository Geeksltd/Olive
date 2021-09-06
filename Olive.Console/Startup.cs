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
        public static IHostEnvironment Environment { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected Startup(IConfiguration config)
        {
            Configuration = config;
            ConfigureServices(Application.ServiceCollection);
            Context.Initialize(Application.ServiceCollection.BuildServiceProvider(), null);

            Environment = Context.Current.GetService<IHostEnvironment>();
            Log.Init(Context.Current.GetService<ILoggerFactory>());
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Configuration.MergeEnvironmentVariables();
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken) => OnStart();

        internal virtual async Task OnStart()
        {
            await Run();
            Process.GetCurrentProcess().Kill();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected abstract Task Run();
    }
    public abstract class UnitTestStartup : Startup
    {
        protected UnitTestStartup(IConfiguration config) : base(config)
        {
        }

        internal override Task OnStart()
        {
            return Task.CompletedTask;
        }

        protected override Task Run()
        {
            return Task.CompletedTask;
        }
    }
}