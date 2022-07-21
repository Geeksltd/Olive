using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Olive.Aws
{
    public abstract class Startup : Mvc.Startup
    {
        protected Startup(IWebHostEnvironment env, IConfiguration config) : base(env, config, null)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture =
                   CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = GetRequestCulture();
        }

        protected override CultureInfo GetRequestCulture() => new("en-GB");

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddCors(c => c.AddPolicy("AllowOrigin",
                options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials()
            ));

            if (Environment.IsProduction())
                services.AddDataProtection().PersistKeysToAWSSystemsManager(Configuration["Aws:Secrets:Id"]);
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