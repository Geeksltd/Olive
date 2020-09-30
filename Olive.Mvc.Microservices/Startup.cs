using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Olive.Mvc.Microservices
{
    public abstract class Startup : Olive.Mvc.Startup
    {
        const string HubDevUrl = "http://localhost:9011";

        protected Startup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory)
            : base(env, config, factory) { }

        public override void ConfigureServices(IServiceCollection services)
        {
            Configuration.MergeEnvironmentVariables();
            services.AddCors(x => x.AddPolicy("AllowHubOrigin",
                f => f.WithOrigins(Microservice.Of("Hub").Url().TrimEnd("/"), HubDevUrl)
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

            base.ConfigureServices(services);
        }

        public override void Configure(IApplicationBuilder app)
        {
            //add menu route
            app.Map("/api/menu", x => x.Run(async context => { await MenuApiMiddleWare.Menu(context); }));

            app.UseCors("AllowHubOrigin");

            //fix CORS issue            
            app.UseMiddleware<MaintainCorsHeader>();

            base.Configure(app);
            Console.Title = Microservice.Me.Name;
        }
    }
}