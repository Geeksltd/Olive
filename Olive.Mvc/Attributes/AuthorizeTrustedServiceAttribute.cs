using Microsoft.AspNetCore.Authorization;

namespace Olive.Mvc
{
    /// <summary>
    /// A strongly typed shortcut to set [Authorize(Roles = «TrustedService»)].
    /// </summary>
    public class AuthorizeTrustedServiceAttribute : AuthorizeAttribute
    {
        public AuthorizeTrustedServiceAttribute() => Roles = "TrustedService";
    }
}
