using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Olive.Mvc
{
    /// <summary>
    /// A strongly typed shortcut to set [Authorize(Roles = (comma seperated access keys))].
    /// </summary>
    public static class MicroserviceAccessKeyAuthentication
    {
        public static IApplicationBuilder UseMicroserviceAccessKeyAuthentication(this IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                var clientId = context.Request.Headers["Microservice.AccessKey"].ToString("|");

                if (clientId.HasValue())
                {
                    var claims = GetRoles(clientId).SelectMany(x => x.Split(',')).Trim()
                    .Select(r => new Claim(ClaimTypes.Role, r))
                    .Concat(new Claim(ClaimTypes.Name, "Microservice.ApiUser"));

                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Service"));
                }

                return next();
            });

            return app;
        }

        /// <summary>
        /// Whether the access key belongs to a configured client, i.e. the request comes from another
        /// microservice rather than from anyone who can set a header.
        /// </summary>
        public static bool IsKnownClient(string clientId) => clientId.HasValue() && GetRoles(clientId).Any();

        static IEnumerable<string> GetRoles(string clientId)
        {
            var provider = Context.Current.GetOptionalService<PermissionsProvider>() ??
                new AppSettingsPermissionsProvider();

            return provider.GetRoles(clientId);
        }

        public abstract class PermissionsProvider
        {
            public abstract IEnumerable<string> GetRoles(string clientId);
        }

        class AppSettingsPermissionsProvider : PermissionsProvider
        {
            public override IEnumerable<string> GetRoles(string clientId)
            {
                var expectedSection = "Authentication:Api.Clients:" + clientId;

                var section = Config.GetSection(expectedSection);

                if (section == null)
                {
                    Console.WriteLine($"Invalid request: {expectedSection} is not defined in appSettings.");
                    return new string[0];
                }

                return section.GetChildren().Select(x => x.Value);
            }
        }
    }
}