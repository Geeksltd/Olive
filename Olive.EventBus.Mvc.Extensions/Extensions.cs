using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public static class Extensions
    {
        public static IApplicationBuilder PullAll<T>(this IApplicationBuilder app, Assembly domainAssembly)
            where T : DestinationEndpoint
        {
            app.Use(async (context, next) =>
            {
                if (!context.Request.Path.Value.ToLower().EndsWithAny(".png", ".jpg", ".xml", ".css", ".js"))
                {
                    var start = LocalTime.Now;
                    Log.For<T>().Info("Pulling all");
                    await ((T)Activator.CreateInstance(typeof(T), domainAssembly)).PullAll();
                    Log.For<T>().Info("Pulled all in " + LocalTime.Now.Subtract(start).ToNaturalTime());
                }
                await next();
            });

            return app;
        }
        static List<string> ExposedEndpoints = new List<string>();
        static bool RegisteredExposedEndpionts;
        const string EXPOSED_ENDPOINTS_ACTION_PREFIX = "/olive/entities/replication/dump/";
        /// <summary>
        /// Registers an endpoint. To view all registered endpoints you can call /olive/entities/replication/dump/all
        /// </summary>
        /// <typeparam name="TSourceEndpoint"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder RegisterPublisher<TSourceEndpoint>(this IApplicationBuilder app)
        where TSourceEndpoint : SourceEndpoint, new()
        {
            var endpoint = new TSourceEndpoint();

            endpoint.Publish(false);

            void Register(string key, Func<HttpContext, Task> handler)
            {
                var action = EXPOSED_ENDPOINTS_ACTION_PREFIX + endpoint.GetType().FullName.Replace(".", "-") + "/" + key.Replace(".", "-");
                ExposedEndpoints.Add(action);
                app.Map(action, x => x.Use(async (context, next) =>
                {
                    var start = LocalTime.Now;
                    Log.For<TSourceEndpoint>().Info("Pulling refresh messages ...");
                    await handler(context);
                    Log.For<TSourceEndpoint>().Info("Pulled all in " + LocalTime.Now.Subtract(start).ToNaturalTime());
                }));
            }

            if (!RegisteredExposedEndpionts)
            {
                RegisteredExposedEndpionts = true;
                Log.For<TSourceEndpoint>().Info("Registering the /all action");
                app.Map(EXPOSED_ENDPOINTS_ACTION_PREFIX + "all", x => x.Use(async (context, next) =>
                    {
                        await context.Response.WriteHtmlAsync(ExposedEndpoints.Select(e => $"<a href='{e}'>{e}</a>").ToHtmlLines());
                    }));
                Log.For<TSourceEndpoint>().Info("Registered the /all action");
            }

            Log.For<TSourceEndpoint>().Info("Registering refresh messages for All ...");
            Register("All", async context =>
            {
                await endpoint.UploadAll();
                await context.Response.WriteHtmlAsync("All done!");
            });
            Log.For<TSourceEndpoint>().Info("Registered refresh messages for All ...");

            return app;
        }

        static Task WriteHtmlAsync(this HttpResponse response, string html)
        {
            response.ContentType = "text/html";
            return response.WriteAsync(html);
        }

    }
}
