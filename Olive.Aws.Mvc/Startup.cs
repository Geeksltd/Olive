using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Olive.Aws
{
	public abstract class Startup
	{
		protected IServiceCollection ServiceCollection { get; private set; }
		public IServiceProvider Services { get; private set; }
		public IHostEnvironment Environment { get; private set; }
		public IConfiguration Configuration { get; private set; }
		protected IHost Host { get; private set; }

		protected Startup()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture =
				   CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = GetRequestCulture();

			var host = new HostBuilder();
			host.ConfigureAppConfiguration((context, builder) =>
			{
				Environment = context.HostingEnvironment;
				builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
				builder.AddEnvironmentVariables();
				ConfigureConfiguration(context, builder);
				Configuration = builder.Build();
			});

			host.ConfigureServices((hostContext, services) =>
			{
				ServiceCollection = services;
				services.AddOptions();
				Configuration.MergeEnvironmentVariables();
				ConfigureServices(services);
				Services = services.BuildServiceProvider();
				Context.Initialize(Services, null);
			});

			host.ConfigureLogging((context, logging) =>
			{
				logging.AddConfiguration(Configuration.GetSection("Logging"));

				if (Environment.IsDevelopment()) logging.AddConsole();
				else
				{
					logging.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
					logging.SetMinimumLevel(LogLevel.Debug);
				}

				ConfigureLogging(context, logging);
				Log.Init(Context.Current.GetService<ILoggerFactory>());
			});

			ConfigureHost(host);

			Host = host.Build();
		}

		protected virtual CultureInfo GetRequestCulture() => new("en-GB");

		protected virtual void ConfigureHost(HostBuilder hostBuilder) { }

		protected virtual void ConfigureServices(IServiceCollection services)
		{
			services.AddCors(c => c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin()));
			services.AddRazorPages().AddRazorPagesOptions(ConfigureRazorPages);
		}

		protected virtual void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder) { }

		protected virtual void ConfigureConfiguration(HostBuilderContext context, IConfigurationBuilder builder) { }

		protected virtual void ConfigureRazorPages(RazorPagesOptions options) { }

		protected string AwsServiceUrl => Configuration["Aws:ServiceUrl"];
	}
}