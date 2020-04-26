using Microsoft.AspNetCore.Builder;
using System;
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

            void Register(string key, Func<Task> handler)
            {
                var action = "/olive/entities/replication/dump/" + endpoint.GetType().FullName.Replace(".", "-") + "/" + key.Replace(".", "-");

                app.Map(action, x => x.Use(async (context, next) =>
                {
                    var start = LocalTime.Now;
                    Log.For<TSourceEndpoint>().Info("Pulling refresh messages ...");
                    await handler();
                    Log.For<TSourceEndpoint>().Info("Pulled all in " + LocalTime.Now.Subtract(start).ToNaturalTime());

                    await next();
                }));
            }

            Log.For<TSourceEndpoint>().Info("Registering refresh messages for All ...");
            Register("All", endpoint.UploadAll);
            Log.For<TSourceEndpoint>().Info("Registered refresh messages for All ...");

            endpoint.ExposedTypes.Do(t =>
            {
                Log.For<TSourceEndpoint>().Info($"Registering refresh messages for {t} ...");
                Register(t, () => endpoint.UploadAll(t));
                Log.For<TSourceEndpoint>().Info($"Registered refresh messages for {t} ...");
            });

            return app;
        }
    }
}
