using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            builder.Build();
        }

        protected virtual void Init(IHostBuilder builder)
        {
            var startup = new TStartup();

            builder.ConfigureAppConfiguration((context, builder) =>
            {
                startup.ConfigureConfiguration(context, builder);
                Config.SetConfiguration(context.Configuration);
            });
            builder.ConfigureServices(services =>
            {
                startup.ConfigureServices(services);
                Context.Initialize(services.BuildServiceProvider(), null);
                Context.Current.GetService<IConfiguration>().MergeEnvironmentVariables();
            });
            builder.ConfigureLogging((context, builder) =>
            {
                startup.ConfigureLogging(context, builder);
                Log.Init(Context.Current.GetService<ILoggerFactory>());
            });

            startup.ConfigureHost(builder);
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