using Microsoft.AspNetCore.Http;
using Olive;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    class SplitRoleClaimsMiddleware : BaseMiddleware
    {
        public SplitRoleClaimsMiddleware(RequestDelegate next) : base(next) { }

        public override async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
                SplitRoles(context.User);
            else if (context.Request.IsLocal() && context.User.Identity is ClaimsIdentity identity)
                ReplaceClaims(identity, new Claim(ClaimTypes.Role, "Local.Request"));

            await Next.Invoke(context);
        }

        void SplitRoles(ClaimsPrincipal user)
        {
            var splitRoles = user.GetRoles().Trim()
                .Select(x => new Claim(ClaimTypes.Role, x))
                .ToArray();

            if (user.Identity is ClaimsIdentity identity)
                ReplaceClaims(identity, splitRoles);
        }

        void ReplaceClaims(ClaimsIdentity identity, params Claim[] splitRoles)
        {
            var old = identity.Claims.Where(x => x.Type == ClaimTypes.Role).ToArray();
            old.Do(x => identity.RemoveClaim(x));
            identity.AddClaims(splitRoles);
        }
    }
}