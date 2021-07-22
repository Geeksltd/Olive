using Amazon.Lambda.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Olive.Aws
{
    public abstract class App<TStartup> : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction where TStartup : Startup
    {
        protected IConfiguration Configuration { get; private set; }

        protected override void Init(IWebHostBuilder builder)
        {
            ConfigureBuilder(builder);
        }

        public ILogger Log => Olive.Log.For(this);

        protected static void Run(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);
            ConfigureBuilder(builder);
            builder.Build().Run();
        }

        static void ConfigureBuilder(IWebHostBuilder builder)
        {
            builder
                .UseStartup<TStartup>()
                .ConfigureLogging(ConfigureLogging);
        }

        static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
        {
            if (context.HostingEnvironment.IsDevelopment()) return;
            logging.AddLambdaLogger(new LambdaLoggerOptions
            {
                IncludeCategory = true,
                IncludeScopes = true,
                IncludeLogLevel = true,
                IncludeEventId = true,
                IncludeException = true,
                IncludeNewline = true,
            });
        }
    }
}