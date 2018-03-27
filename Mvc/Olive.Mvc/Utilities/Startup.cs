using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Olive.Entities;
using Olive.Security;

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
            Context.Initialize(services);

            services.AddSingleton(typeof(IHttpContextAccessor), typeof(HttpContextAccessor))
               .AddSingleton(typeof(IActionContextAccessor), typeof(ActionContextAccessor));

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

            services.ConfigureApplicationCookie(ConfigureApplicationCookie);

            AuthenticationBuilder = services.AddAuthentication(config => config.DefaultScheme = "Cookies")
                .AddCookie(ConfigureApplicationCookie);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Context.Current.Configure(app.ApplicationServices).Configure(env);
            ConfigureExceptionPage(app, env);
            InstantiateDatabase(app, env);

            app.UseMicroserviceAccessKeyAuthentication()
                .UseAuthentication()
                .UseStaticFiles()
                .UseRequestLocalization(RequestLocalizationOptions)
                .UseMvc(ConfigureRoutes);
        }

        protected virtual void InstantiateDatabase(IApplicationBuilder app, IHostingEnvironment env)
            => Entity.InitializeDatabase(Entities.Data.Database.Instance);

        protected virtual void ConfigureExceptionPage(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() || env.IsStaging())
                app.UseDeveloperExceptionPage().UseBrowserLink();
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

        protected virtual void ConfigureApplicationCookie(CookieAuthenticationOptions options)
        {
            options.AccessDeniedPath = options.LoginPath = "/login";
            options.LogoutPath = "/lLogout";
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = ".myAuth";

            if (Config.Get("Authentication:CookieEncryptionKey")
                .Or(Config.Get("Authentication:CookieDecryptionKey")).HasValue())
            {
                options.DataProtectionProvider = new AsymmetricKeyDataProtector("AuthCookies");
            }
            else if (Config.Get("Authentication:CookieDataProtectorKey").HasValue())
            {
                options.DataProtectionProvider = new SymmetricKeyDataProtector("AuthCookies");
            }

            options.SlidingExpiration = true;
            options.Cookie.Expiration = TimeSpan.FromMinutes(Config.Get("Authentication:Cookie:Timeout", 20.0));
        }
    }
}