using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Olive.Mvc
{
    static class CorsByConfig
    {
        static bool Configured;

        public static void FromConfig(this CorsOptions options, IConfiguration config)
        {
            var allowed = config["cors:allow"].ToLowerOrEmpty().Split(",").Trim().Distinct().ToArray();

            if (allowed.None()) return;
            Configured = true;

            options.AddPolicy("AllowedByConfig", builder =>
            {
                builder.WithOrigins(allowed)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        }

        public static void UseCorsFromConfig(this IApplicationBuilder app)
        {
            if (Configured)
                app.UseCors("AllowedByConfig");
        }
    }
}