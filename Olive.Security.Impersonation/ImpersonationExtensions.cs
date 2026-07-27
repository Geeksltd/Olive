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

        static bool IsRequiredToEndImpersonation(Claim claim)
        {
            return claim.Type.IsAnyOf(
                ClaimTypes.NameIdentifier.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Name.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Email.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Role.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX),
                ClaimTypes.Expiration,
                ClaimTypes.IsPersistent);
        }

        /// <summary>
        /// Determines whether this principal is an impersonated one.
        /// </summary>
        /// <remarks>
        /// Detected from the preserved impersonator claims rather than the IMPERSONATOR role. Middleware
        /// that rewrites role claims - for example a distributed role store that re-fetches them on every
        /// request - discards that role marker, but leaves the prefixed claims alone.
        /// </remarks>
        public static bool IsImpersonated(this ClaimsPrincipal @this)
            => @this?.Claims.Any(x => x.Type.StartsWith(IMPERSONATOR_CLAIMS_PREFIX)) == true;

        /// <summary>Gets the display name of the user who started this impersonation, or empty.</summary>
        public static string GetImpersonatorName(this ClaimsPrincipal @this)
            => @this.GetImpersonatorClaim(ClaimTypes.Name);

        /// <summary>Gets the ID of the user who started this impersonation, or empty.</summary>
        public static string GetImpersonatorId(this ClaimsPrincipal @this)
            => @this.GetImpersonatorClaim(ClaimTypes.NameIdentifier);

        /// <summary>Gets the email of the user who started this impersonation, or empty.</summary>
        public static string GetImpersonatorEmail(this ClaimsPrincipal @this)
            => @this.GetImpersonatorClaim(ClaimTypes.Email);

        static string GetImpersonatorClaim(this ClaimsPrincipal @this, string claimType)
            => @this?.Claims.FirstOrDefault(x => x.Type == claimType.WithPrefix(IMPERSONATOR_CLAIMS_PREFIX))
                ?.Value ?? "";
    }
}
