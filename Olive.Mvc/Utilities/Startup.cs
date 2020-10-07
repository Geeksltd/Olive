using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Olive.Entities;
using Olive.Entities.Data;
using Olive.Security;

namespace Olive.Mvc
{
    public abstract class Startup
    {
        const int DEFAULT_SESSION_TIMEOUT = 20;

        protected readonly IWebHostEnvironment Environment;
        protected readonly IConfiguration Configuration;
        protected IServiceCollection Services { get; private set; }

        protected Startup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
        {
            Environment = env;
            Configuration = config;
            Log.Init(loggerFactory);
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Configuration.MergeEnvironmentVariables();
            Services = services;
            services.AddHttpContextAccessor();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddTransient<IFileAccessorFactory, FileAccessorFactory>();
            services.AddDatabase(Configuration);
            services.AddHttpClient();

            ConfigureMvc(services.AddMvc());

            services.AddSingleton<IValidationAttributeAdapterProvider, OliveValidationAttributeAdapterProvider>();

            services.AddResponseCompression();
            services.AddResponseCaching();
            services.AddDefaultAudit();

            services.Configure<RazorViewEngineOptions>(o =>
            o.ViewLocationExpanders.Add(new ViewLocationExpander()));

            services.TryAddTransient<IFileRequestService, DiskFileRequestService>();
            services.TryAddTransient<IFileUploadMarkupGenerator, DefaultFileUploadMarkupGenerator>();

            ConfigureAuthentication(services.AddAuthentication(config => config.DefaultScheme = "Cookies"));

            services.AddControllersWithViews().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);
            // Caused "Urecognized SameSiteMode value -1
            //services.ConfigureNonBreakingSameSiteCookies();
        }

        protected virtual void ConfigureAuthentication(AuthenticationBuilder auth)
        {
            auth.AddCookie("Cookies", options => ConfigureAuthCookie(options));
        }

        protected virtual void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.AddMvcOptions(x => x.ModelBinderProviders.Insert(0, new OliveBinderProvider()));
            mvc.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

            //mvc.ConfigureApplicationPartManager(manager =>
            //{
            //    manager.FeatureProviders.RemoveWhere(x => x is MetadataReferenceFeatureProvider);
            //    manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
            //});

            mvc.AddMvcOptions(options =>
            {
                options.ModelMetadataDetailsProviders.Add(
                    new SuppressChildValidationMetadataProvider(typeof(IEntity)));

                options.EnableEndpointRouting = false;
            });
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<PerformanceMonitoringMiddleware>();

            Context.Initialize(app.ApplicationServices, () => app.ApplicationServices.GetService<IHttpContextAccessor>()?.HttpContext?.RequestServices);
            Context.Current.GetService<IDatabaseProviderConfig>().Configure();

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
            app.UseCookiePolicy();
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