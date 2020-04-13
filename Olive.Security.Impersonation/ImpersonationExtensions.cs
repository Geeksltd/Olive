using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace Olive.Security
{
    public static class ImpersonationExtensions
    {
        const string IMPERSONATOR_CLAIMS_PREFIX = "olive-impersonator-id-";

        internal static IEnumerable<Claim> ToImpersonatorClaims(this ClaimsPrincipal @this)
        {
            foreach (var item in @this.Claims.Where(x => x.Type.IsAnyOf(
                ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email, ClaimTypes.Role)))
            {
                yield return new Claim(item.Type.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX), item.Value);
            }
        }

        internal static ClaimsPrincipal FromImpersonatorClaims(this ClaimsPrincipal @this)
        {
            var claims = new List<Claim>();

            foreach (var item in @this.Claims.Where(x => IsRequiredToEndImpersonation(x)))
                claims.Add(new Claim(item.Type.TrimStart(IMPERSONATOR_CLAIMS_PREFIX), item.Value));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Olive"));
        }

        private static bool IsRequiredToEndImpersonation(Claim claim)
        {
            return claim.Type.IsAnyOf(
                ClaimTypes.NameIdentifier.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Name.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Email.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Role.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Expiration,
                ClaimTypes.IsPersistent);
        }

        public static string GetImpersonatorName(this ClaimsPrincipal @this)
        {
            return @this?.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.Name.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX))
                ?.Value ?? "";
        }
    }
}
