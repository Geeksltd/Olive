using Microsoft.AspNetCore.Builder;
using System;
using System.Net;
using System.Reflection;

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

    }
}
