using Amazon.Lambda.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Olive.Aws
{
    public abstract class App<TApp, TStartup> : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        where TApp : App<TApp, TStartup>, new()
        where TStartup : Startup
    {
        protected IConfiguration Configuration { get; private set; }

        protected override void Init(IWebHostBuilder builder)
        {
            ConfigureBuilder(builder);
        }

        public ILogger Log => Olive.Log.For(this);

        protected static void LocalRun(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);
            new TApp().Init(builder);
            builder.Build().Run();
        }

        protected virtual void ConfigureBuilder(IWebHostBuilder builder)
        {
            builder
                .UseStartup<TStartup>()
                .ConfigureLogging(ConfigureLogging);
        }

        protected virtual void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
        {
            if (context.HostingEnvironment.IsDevelopment()) return;
            logging.AddLambdaLogger(context.Configuration, "Logging");
        }
    }
}