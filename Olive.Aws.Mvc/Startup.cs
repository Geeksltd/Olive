using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Olive.Aws
{
    public abstract class Startup : Mvc.Startup
    {
        protected Startup(IWebHostEnvironment env, IConfiguration config) : base(env, config)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture =
                   CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = GetRequestCulture();
        }

        protected override CultureInfo GetRequestCulture() => new("en-GB");

        public override void ConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory)
        {
            base.ConfigureServices(services, loggerFactory);

            services.AddCors(c => c.AddPolicy("AllowOrigin",
                options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
            ));

            if (!Environment.IsProduction()) return;

            var secretId = Configuration["Aws:Secrets:Id"];
            if (secretId.IsEmpty()) return;

            services.AddDataProtection().PersistKeysToAWSSystemsManager(secretId);
        }

        protected override void ConfigureMvc(IMvcBuilder mvc)
        {
            base.ConfigureMvc(mvc);
            mvc.AddRazorPagesOptions(ConfigureRazorPagesOptions);
        }

        protected virtual void ConfigureRazorPagesOptions(Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions options) { }

        protected string AwsServiceUrl => Configuration["Aws:ServiceUrl"];
    }
}