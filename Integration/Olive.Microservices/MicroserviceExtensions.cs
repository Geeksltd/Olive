using System.Security.Claims;

namespace Olive
{
    public static class MicroserviceExtensions
    {
        /// <summary>
        /// Determines whether this user is in role TrustedService.
        /// </summary>        
        public static bool IsTrustedService(this ClaimsPrincipal user)
        {
            return user.IsInRole("TrustedService");
        }
    }
}