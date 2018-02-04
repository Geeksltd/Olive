using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Olive
{
    partial class OliveExtensions
    {
        public static string GetEmail(this ClaimsPrincipal principal)
            => principal.Claims.FirstOrDefault(x => x.ValueType == ClaimTypes.Email)?.Value;

        public static string GetId(this ClaimsPrincipal principal)
            => principal.Claims.FirstOrDefault(x => x.ValueType == ClaimTypes.NameIdentifier)?.Value;

        public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
            => principal.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).Trim();

        public static string GetFirstIssuer(this ClaimsPrincipal principal)
            => principal?.Claims?.Select(x => x.Issuer).Trim().FirstOrDefault();
    }
}