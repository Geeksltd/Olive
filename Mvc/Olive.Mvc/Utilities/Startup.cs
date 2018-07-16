using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Olive.Entities;
using Olive.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public abstract class Startup
    {
        protected AuthenticationBuilder AuthenticationBuilder;

        protected virtual IViewLocationExpander GetViewLocationExpander() => new ViewLocationExpander();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application,
        // visit https://go.microsoft.com/fwlink/?LinkID=398940
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IHttpContextAccessor), typeof(HttpContextAccessor))
               .AddSingleton(typeof(IActionContextAccessor), typeof(ActionContextAccessor));

            Context.Initialize(services);
            services.AddSingleton<IDatabase>(new Entities.Data.Database());

            var mvc = services.AddMvc(o => o.ModelBinderProviders.Insert(0, new OliveBinderProvider()));
            Context.Current.AddService(typeof(IMvcBuilder), mvc);

            mvc.AddJsonOptions(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());

            mvc.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.RemoveWhere(x => x is MetadataReferenceFeatureProvider);
                manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
            });

            services.Configure<RazorViewEngineOptions>(options =>
                options.ViewLocationExpanders.Add(GetViewLocationExpander()));

            services.AddResponseCompression(ConfigureResponseCompressionOptions);

            AuthenticationBuilder = services.AddAuthentication(config => config.DefaultScheme = "Cookies")
                .AddCookie(ConfigureAuthCookie);
        }

        protected virtual void ConfigureResponseCompressionOptions(ResponseCompressionOptions options) { }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Context.Current.Configure(app.ApplicationServices).Configure(env);
            app.UseResponseCompression();

            app.UseMiddleware<AsyncStartupMiddleware>((Func<Task>)(() => OnStartUpAsync(app, env)));

            ConfigureExceptionPage(app, env);
            ConfigureSecurity(app, env);
            ConfigureRequestHandlers(app, env);
        }

        public virtual Task OnStartUpAsync(IApplicationBuilder app, IHostingEnvironment env)
        {
            return Task.CompletedTask;
        }

        protected virtual void ConfigureSecurity(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMicroserviceAccessKeyAuthentication();
            app.UseAuthentication();
        }

        protected virtual void ConfigureRequestHandlers(IApplicationBuilder app, IHostingEnvironment env)
        {
            UseStaticFiles(app, env);
            app.UseRequestLocalization(RequestLocalizationOptions);
            app.UseMvc();
        }

        protected virtual void UseStaticFiles(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseStaticFilesCaseSensitive();
            else app.UseStaticFiles();
        }

        protected virtual void ConfigureExceptionPage(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() || env.IsStaging())
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

            var expireTime = TimeSpan.FromMinutes(Config.Get("Authentication:Cookie:Timeout", 20.0));
            options.ExpireTimeSpan = expireTime;
            options.Cookie.MaxAge = expireTime;
        }
    }
}