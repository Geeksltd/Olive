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
using Microsoft.Extensions.Options;
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

        protected readonly IWebHostEnvironment Environment;
        protected readonly IConfiguration Configuration;

        protected IServiceCollection Services { get; private set; }

        protected Startup(IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = Config.Build();
            Log.Init(builder =>
            {
                builder.AddConsole();
            });
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Configuration.MergeEnvironmentVariables();

            Services = services;

            services.AddSingleton<IConfiguration>((IConfigurationRoot)Configuration);
            services.AddSingleton<ILoggerFactory>(Log.Factory);

            services.AddHttpContextAccessor();
            services.AddCors(opt => opt.FromConfig(Configuration));

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddTransient<IFileAccessorFactory, FileAccessorFactory>();
            services.AddDatabase(Configuration);
            services.AddHttpClient();

            ConfigureMvc(services.AddMvc());

            services.AddSingleton<IValidationAttributeAdapterProvider, OliveValidationAttributeAdapterProvider>();

            services.AddResponseCompression();
            services.AddResponseCaching();
            services.AddDefaultAudit();

            services.Configure<RazorViewEngineOptions>(o => o.ViewLocationExpanders.Add(new ViewLocationExpander()));

            services.TryAddTransient<IFileRequestService, DiskFileRequestService>();
            services.TryAddTransient<IFileUploadMarkupGenerator, DefaultFileUploadMarkupGenerator>();

            ConfigureAuthentication(services
                .AddAuthentication(config =>
                {
                    config.DefaultScheme = "SmartScheme";
                })
                .AddPolicyScheme("SmartScheme", "Bearer-to-Cookie Proxy", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        var cookieOptions = context.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
                        var cookieName = cookieOptions.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name;

                        if (!cookieName.IsEmpty() && !authHeader.IsEmpty() && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            var token = authHeader.Substring("Bearer ".Length).Trim();
                            context.Request.Headers.Append("Cookie", $"{cookieName}={token}");
                        }

                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                })
            );

            services.AddControllersWithViews().AddJsonOptions(ConfigureJsonOptions);
            // Caused "Unrecognized SameSiteMode value -1
            //services.ConfigureNonBreakingSameSiteCookies();
        }

        protected virtual void ConfigureJsonOptions(Microsoft.AspNetCore.Mvc.JsonOptions options) =>
            options.JsonSerializerOptions.PropertyNamingPolicy = null;

        protected virtual void ConfigureAuthentication(AuthenticationBuilder auth)
        {
            auth.AddCookie("Cookies", options => ConfigureAuthCookie(options));
        }

        protected virtual void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.AddMvcOptions(x => x.ModelBinderProviders.Insert(0, new OliveBinderProvider()));
            mvc.AddJsonOptions(ConfigureJsonOptions);

            mvc.AddMvcOptions(options =>
            {
                options.ModelMetadataDetailsProviders.Add(
                    new SuppressChildValidationMetadataProvider(typeof(IEntity)));

                options.EnableEndpointRouting = false;
            });
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseLogUnhandledExceptionsMiddleware();

            Context.Initialize(app.ApplicationServices, () => app.ApplicationServices.GetService<IHttpContextAccessor>()?.HttpContext?.RequestServices);
            Context.Current.GetService<IDatabaseProviderConfig>().Configure();

            app.UseCorsFromConfig();

            ConfigureRequestHandlers(app);
        }

        public virtual Task OnStartUpAsync(IApplicationBuilder app) => Task.CompletedTask;

        protected virtual void ConfigureSecurity(IApplicationBuilder app)
        {
            app.UseCookiePolicy();
            ConfigureMicroserviceSecurity(app);
            app.UseAuthentication();
            app.UseMiddleware<SplitRoleClaimsMiddleware>();
        }

        protected virtual void ConfigureMicroserviceSecurity(IApplicationBuilder app) => app.UseMicroserviceAccessKeyAuthentication();

        protected virtual void ConfigureRequestHandlers(IApplicationBuilder app)
        {
            app.UseResponseCompression();
            UseStaticFiles(app);
            ConfigureExceptionPage(app);
            ConfigureSecurity(app);
            ConfigureMiddlewares(app);
            app.UseRequestLocalization(RequestLocalizationOptions);
            app.UseMvc();
        }

        protected virtual void ConfigureMiddlewares(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment()) app.UseMiddleware<DevCommandMiddleware>();
            app.UseMiddleware<AsyncStartupMiddleware>((Func<Task>)(() => OnStartUpAsync(app)));
            app.UseMiddleware<PerformanceMonitoringMiddleware>();
        }

        protected virtual void UseStaticFiles(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment()) app.UseStaticFilesCaseSensitive();
            else app.UseStaticFiles(GetStaticFileOptions());
        }

        protected virtual StaticFileOptions GetStaticFileOptions()
        {
            const int OneDay = 60 * 60 * 24;
            const int OneYear = OneDay * 365;

            return new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var cacheDuration = ctx.Context.Request.Query.ContainsKey("v") ? OneYear : OneDay;
                    ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=" + cacheDuration + ",immutable";
                }
            };
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

            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = ctx =>
                {
                    if (IsApiRequest(ctx.Request))
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    else
                        ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = ctx =>
                {
                    if (IsApiRequest(ctx.Request))
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    else
                        ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                }
            };
        }

        static bool IsApiRequest(HttpRequest request)
        {
            if (request.Path.StartsWithSegments("/api/")) return true;

            var authHeader = request.Headers["Authorization"].ToString();
            if (authHeader.HasValue() && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return true;

            var accept = request.Headers["Accept"].ToString();
            if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)) return true;

            if (request.Headers["X-Requested-With"] == "XMLHttpRequest") return true;
            if (request.Headers["X-Api-Request"] == "true") return true;

            return false;
        }
    }
}