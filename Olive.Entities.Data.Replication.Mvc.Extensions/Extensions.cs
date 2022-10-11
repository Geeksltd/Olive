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
        static bool IsMultiServerMode =>
            (Olive.Config.Get("DataReplication:Mode") ?? throw new Exception("DataReplication:Mode has not be specified. Options: MultiServer, SingleServer")) == "MultiServer";

        const string EVERY_MINUTE_CRON = "* * * * *";

        public static IApplicationBuilder RegisterSubscriber<T>(this IApplicationBuilder app, Assembly domainAssembly)
            where T : DestinationEndpoint
        {

            var endpoint = (T)Activator.CreateInstance(typeof(T), domainAssembly);

            if (IsMultiServerMode)
                Context.Current.GetService<BackgroundJobsPlan>()
                        .Register(new BackgroundJob(typeof(T).FullName, () => endpoint.PullAll(), EVERY_MINUTE_CRON));
            else
                Task.Factory.RunSync(() => endpoint.Subscribe());

            app.Map("/olive-endpoints/" + typeof(T).FullName.ToLower().Replace(".", "-"),
                x => x.Use(async (ctx, next) =>
                {
                    Log.For(typeof(T)).Info("Pulling all trigger by the endpoint handler");
                    await endpoint.PullAll();
                    Log.For(typeof(T)).Info("Pulled all trigger by the endpoint handler");
                }));

            return app;
        }

        //public static IApplicationBuilder PullAll<T>(this IApplicationBuilder app, Assembly domainAssembly)
        //    where T : DestinationEndpoint
        //{
        //    app.Use(async (context, next) =>
        //    {
        //        if (!context.Request.Path.Value.ToLower().EndsWithAny(".png", ".jpg", ".xml", ".css", ".js"))
        //        {
        //            var start = LocalTime.Now;
        //            Log.For<T>().Info("Pulling all");
        //            await ((T)Activator.CreateInstance(typeof(T), domainAssembly)).PullAll();
        //            Log.For<T>().Info("Pulled all in " + LocalTime.Now.Subtract(start).ToNaturalTime());
        //        }
        //        await next();
        //    });

        //    return app;
        //}
        static List<string> ExposedEndpoints = new List<string>();
        static bool RegisteredExposedEndpionts;
        static string EXPOSED_ENDPOINTS_ACTION_PREFIX = Config.Get("DataReplication:DumpUrl", "/olive/entities/replication/dump/");
        /// <summary>
        /// Registers an endpoint. To view all registered endpoints you can call /olive/entities/replication/dump/all
        /// </summary>
        /// <typeparam name="TSourceEndpoint"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder RegisterPublisher(this IApplicationBuilder app, SourceEndpoint endpoint)
        {
            endpoint.Publish(false);
            var logger = Log.For(endpoint);
            var hasDumpURL = Config.Get("DataReplication:AllowDumpUrl", defaultValue: true);

            void Register(string key, Func<HttpContext, Task> handler)
            {
                var action = EXPOSED_ENDPOINTS_ACTION_PREFIX + endpoint.GetType().FullName.Replace(".", "-") + "/" + key.Replace(".", "-");
                ExposedEndpoints.Add(action);
                app.Map(action, x => x.Use(async (context, next) =>
                {
                    var start = LocalTime.Now;
                    logger.Info("Pulling refresh messages ...");
                    await handler(context);
                    logger.Info("Pulled all in " + LocalTime.Now.Subtract(start).ToNaturalTime());
                }));
            }

            if (!RegisteredExposedEndpionts && hasDumpURL)
            {
                RegisteredExposedEndpionts = true;
                logger.Info("Registering the /all action");
                app.Map(EXPOSED_ENDPOINTS_ACTION_PREFIX + "all", x => x.Use(async (context, next) =>
                {
                    await context.Response.WriteHtmlAsync(ExposedEndpoints.Select(e => $"<a href='{e}'>{e}</a>").ToHtmlLines());
                }));
                logger.Info("Registered the /all action");
            }

            logger.Info("Registering refresh messages for All ...");
            Register("All", async context =>
            {
                await endpoint.UploadAll();
                await context.Response.WriteHtmlAsync("All done!");
            });
            logger.Info("Registered refresh messages for All ...");

            return app;
        }
        public static IApplicationBuilder RegisterPublisher<TSourceEndpoint>(this IApplicationBuilder app)
        where TSourceEndpoint : SourceEndpoint, new() => RegisterPublisher(app, new TSourceEndpoint());

        static Task WriteHtmlAsync(this HttpResponse response, string html)
        {
            response.ContentType = "text/html";
            return response.WriteAsync(html);
        }

    }
}
