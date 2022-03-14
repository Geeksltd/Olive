using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Olive.Aws
{
    public abstract class Startup
    {
        protected IConfiguration Configuration;
        protected IServiceCollection Services { get; private set; }

        public virtual void ConfigureConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            Config.SetConfiguration(Configuration = context.Configuration);
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Configuration.MergeEnvironmentVariables();
            Services = services;
            services.AddOptions();
        }

        public virtual void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConfiguration(context.Configuration.GetSection("Logging"));

            if (context.HostingEnvironment.IsDevelopment()) builder.AddConsole();
            else
            {
                builder.AddAWSProvider(context.Configuration.GetAWSLoggingConfigSection());
                builder.SetMinimumLevel(LogLevel.Debug);
            }
        }

        public virtual void ConfigureHost(IHostBuilder builder) { }

        protected string AwsServiceUrl => Configuration["Aws:ServiceUrl"];
    }
}