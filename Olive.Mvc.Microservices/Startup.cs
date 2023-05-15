using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Olive.Mvc.Microservices
{
    public abstract class Startup : Olive.Mvc.Startup
    {
        const string HubDevUrl = "http://localhost:9011";

        protected Startup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory)
            : base(env, config, factory) { }

        public override void ConfigureServices(IServiceCollection services)
        {
            var permittedUrls = Config.Get("PermittedDomains").Split(",").Union(Microservice.Of("Hub").Url()).Select(d => d.TrimEnd("/")).Union(HubDevUrl).ToArray();
            Configuration.MergeEnvironmentVariables();

            services.AddCors(x => x.AddPolicy("AllowHubOrigin",
                f => f.WithOrigins(permittedUrls)
                .SetIsOriginAllowed(x => true)
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

            base.ConfigureServices(services);
        }

        public override void Configure(IApplicationBuilder app)
        {
            app.UseCors("AllowHubOrigin");
            // add menu route
            app.Map("/api/menu", x => x.Run(MenuApiMiddleWare.Menu));

            // get features data automatically
            app.Map("/olive/features", x => x.Run(NavigationApiMiddleWare.Navigate));

            // get board sources
            app.Map("/olive/board/sources", x => x.Run(NavigationApiMiddleWare.BoardSources));

            // fix CORS issue
            app.UseMiddleware<MaintainCorsHeader>();

            base.Configure(app);
            Console.Title = Microservice.Me.Name;
            if (Context.Current.Environment().EnvironmentName == "Development" && !DevelopmentShareInfo.Shared && Microservice.Me.Name != "Hub")
                app.Use(DevelopmentShareInfo.ShareMyData);

            // get dynamic board data
            app.Map("/olive/board/features", x => x.Run(NavigationApiMiddleWare.Search));
        }
    }
}