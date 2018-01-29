using Microsoft.AspNetCore.Authorization;

namespace Olive.Mvc
{
    /// <summary>
    /// A strongly typed shortcut to set [Authorize(Roles = «{servicename}.service»)].
    /// Authorizes a peer micro-service.
    /// </summary>
    public class AuthorizeServiceAttribute : AuthorizeAttribute
    {
        public AuthorizeServiceAttribute(string serviceName) => Roles = serviceName.ToLower() + ".service";
    }
}