using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Olive.Entities;
using Olive.Entities.Data;
using Olive.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public abstract class Startup
    {
        const int DEFAULT_SESSION_TIMEOUT = 20;

        protected readonly IHostingEnvironment Environment;
        protected readonly IConfiguration Configuration;
        protected readonly IServiceCollection Services;

        protected Startup(IHostingEnvironment env, IConfiguration config)
        {
            Environment = env;
            Configuration = config;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Context.Initialize(services);
            services.AddHttpContextAccessor();

            services.AddSingleton(typeof(IActionContextAccessor), typeof(ActionContextAccessor));
            services.AddDatabase();
            services.AddHttpClient();

            ConfigureMvc(services.AddMvc());

            services.AddResponseCompression();
            services.AddResponseCaching();

            services.Configure<RazorViewEngineOptions>(o =>
            o.ViewLocationExpanders.Add(new ViewLocationExpander()));

            ConfigureAuthentication(services.AddAuthentication(config => config.DefaultScheme = "Cookies"));
        }

        protected virtual void ConfigureAuthentication(AuthenticationBuilder auth)
        {
            auth.AddCookie("Cookies", options => ConfigureAuthCookie(options));
        }

        protected virtual void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.AddMvcOptions(x => x.ModelBinderProviders.Insert(0, new OliveBinderProvider()));

            mvc.AddJsonOptions(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());

            mvc.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.RemoveWhere(x => x is MetadataReferenceFeatureProvider);
                manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
            });
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            Context.Current.Set(app.ApplicationServices).Set(Environment);

            app.ApplicationServices.GetService<IDatabase>().Configure();

            if (Environment.IsDevelopment())
                app.UseMiddleware<DevCommandMiddleware>();

            app.UseResponseCompression();

            app.UseMiddleware<AsyncStartupMiddleware>((Func<Task>)(() => OnStartUpAsync(app)));

            ConfigureExceptionPage(app);
            ConfigureSecurity(app);
            ConfigureRequestHandlers(app);
        }

        public virtual Task OnStartUpAsync(IApplicationBuilder app) => Task.CompletedTask;

        protected virtual void ConfigureSecurity(IApplicationBuilder app)
        {
            app.UseMicroserviceAccessKeyAuthentication();
            app.UseAuthentication();
            app.UseMiddleware<SplitRoleClaimsMiddleware>();
        }

        protected virtual void ConfigureRequestHandlers(IApplicationBuilder app)
        {
            UseStaticFiles(app);
            app.UseRequestLocalization(RequestLocalizationOptions);
            app.UseMvc();
        }

        protected virtual void UseStaticFiles(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment()) app.UseStaticFilesCaseSensitive();
            else app.UseStaticFiles();
        }

        protected virtual void ConfigureExceptionPage(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment() || Environment.IsStaging())
                app.UseDeveloperExceptionPage();
            else app.UseExceptionHandler("/error");
        }

        protected virtual CultureInfo GetRequestCulture() => CultureInfo.CurrentCulture;

        protected virtual RequestLocalizationOptions RequestLocalizationOptions
        {
            get
            {
                var culture = GetRequestCulture();

                return new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture),
                    SupportedCultures = new List<CultureInfo> { culture },
                    SupportedUICultures = new List<CultureInfo> { culture }
                };
            }
        }

        protected virtual void ConfigureRoutes(IRouteBuilder routes) { }

        protected virtual void ConfigureAuthCookie(CookieAuthenticationOptions options)
        {
            options.AccessDeniedPath = options.LoginPath = "/login";
            options.LogoutPath = "/lLogout";
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = ".myAuth";

            if (Config.Get("Authentication:CookieDataProtectorKey").HasValue())
            {
                options.DataProtectionProvider = new SymmetricKeyDataProtector(Config.Get("Authentication:CookieDataProtectorKey"));
            }

            options.SlidingExpiration = true;

            var expireTime = Config.Get("Authentication:Cookie:Timeout", DEFAULT_SESSION_TIMEOUT).Minutes();
            options.ExpireTimeSpan = expireTime;
            options.Cookie.MaxAge = expireTime;
        }
    }
}