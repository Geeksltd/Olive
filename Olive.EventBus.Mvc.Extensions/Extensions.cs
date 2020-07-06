using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
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

        public static IApplicationBuilder RegisterPublisher<TSourceEndpoint>(this IApplicationBuilder app)
        where TSourceEndpoint : SourceEndpoint, new()
        {
            var endpoint = new TSourceEndpoint();

            endpoint.Publish(false);

            void Register(string key, Func<HttpContext, Task> handler)
            {
                var action = "/olive/entities/replication/dump/" + endpoint.GetType().FullName.Replace(".", "-") + "/" + key.Replace(".", "-");

                app.Map(action, x => x.Use(async (context, next) =>
                {
                    var start = LocalTime.Now;
                    Log.For<TSourceEndpoint>().Info("Pulling refresh messages ...");
                    await handler(context);
                    Log.For<TSourceEndpoint>().Info("Pulled all in " + LocalTime.Now.Subtract(start).ToNaturalTime());
                }));
            }

            Log.For<TSourceEndpoint>().Info("Registering refresh messages for All ...");
            Register("All", async context =>
            {
                await endpoint.UploadAll();
                context.WriteLine("All done!");
            });
            Log.For<TSourceEndpoint>().Info("Registered refresh messages for All ...");

            endpoint.ExposedTypes.Do(t =>
            {
                Log.For<TSourceEndpoint>().Info($"Registering refresh messages for {t} ...");
                Register(t, async context =>
                 {
                     await endpoint.UploadAll(t);
                     context.WriteLine("All done!");
                 });
                Log.For<TSourceEndpoint>().Info($"Registered refresh messages for {t} ...");
            });

            return app;
        }

        static void WriteLine(this HttpContext context, string text)
        {
            using (var writer = new StreamWriter(context.Response.Body))
                writer.Write(text);
        }
    }
}
