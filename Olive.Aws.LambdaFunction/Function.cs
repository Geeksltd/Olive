using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Olive.Aws
{
    public abstract class Function<TStartup> where TStartup : Startup, new()
    {
        protected Function() : this(null) { }

        protected Function(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            Init(builder);
            var host = builder.Build();
            HostCreated(host);
        }

        protected virtual void Init(IHostBuilder builder)
        {
            var startup = new TStartup();

            builder.ConfigureAppConfiguration(startup.ConfigureConfiguration);
            builder.ConfigureServices(startup.ConfigureServices);
            builder.ConfigureLogging(startup.ConfigureLogging);

            startup.ConfigureHost(builder);
        }

        protected virtual void HostCreated(IHost host)
        {
            Context.Initialize(host.Services, null);

            var configuration = Context.Current.GetService<IConfiguration>();
            Config.SetConfiguration(configuration);
            configuration.MergeEnvironmentVariables();

            Log.Init(Context.Current.GetService<ILoggerFactory>());
        }

        protected static Task LocalExecute<TFunction>(string[] args)
            where TFunction : Function<TStartup>, new()
        {
            var function = (TFunction)Activator.CreateInstance(typeof(TFunction), args);
            return function.ExecuteAsync(null);
        }

        public abstract Task ExecuteAsync(ILambdaContext context);
    }
}