using Amazon.Lambda.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Amazon.Lambda.AspNetCoreServer;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Olive.Aws
{
    public abstract class App<TStartup> : APIGatewayProxyFunction
        where TStartup : Startup
    {
        protected IConfiguration Configuration { get; private set; }

        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<TStartup>()
                .ConfigureLogging(ConfigureLogging);
        }

        protected static void LocalRun<TApp>(string[] args)
            where TApp : App<TStartup>, new()
        {
            LocalRun(new TApp(), args);
        }

        protected static void LocalRun<TApp>(TApp app, string[] args)
            where TApp : App<TStartup>
        {
            var builder = WebHost.CreateDefaultBuilder(args);
            app.Init(builder);
            builder.Build().Run();
        }

        protected virtual void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
        {
            logging.ClearProviders();

            logging.AddConfiguration(context.Configuration.GetSection("Logging"));

            if (context.HostingEnvironment.IsDevelopment()) logging.AddConsole();
            else
            {
                logging.AddLambdaLogger(new LambdaLoggerOptions
                {
					IncludeCategory = true,
                    IncludeLogLevel = true,
                    IncludeNewline = true,
                    IncludeEventId = true,
                    IncludeScopes = true,
                    IncludeException = true
                });

                logging.SetMinimumLevel(LogLevel.Debug);
            }
        }
    }
}