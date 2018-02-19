using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Olive.Mvc
{
    /// <summary>
    /// A strongly typed shortcut to set [Authorize(Roles = (comma seperated access keys))].
    /// </summary>
    public class AuthorizeServiceClientAttribute : AuthorizeAttribute
    {
        string RequiredPermission;
        public AuthorizeServiceClient(string requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }

        void SomeFilter()
        {
            var clientId = Context.Current.Request().Headers["Microservice.AccessKey"];
            var provider = Context.Current.GetOptionalService<PermissionsProvider>() ?? new AppSettingsPermissionsProvider();
            Roles = provider.GetPermissions(clientId).ToString(",");
        }

        public abstract class PermissionsProvider
        {
            public abstract string[] GetPermissions(string clientId);
        }

        class AppSettingsPermissionsProvider : PermissionsProvider
        {
            public override string[] GetPermissions(string clientId)
            {
                return Config.GetSectionOrThrow("Authentication:Api.Clients:" + clientId)
                    .GetChildren()
                    .SelectMany(x => x.GetChildren()).Select(x => x.Value)
                    .ToArray();
            }
        }
    }


}
