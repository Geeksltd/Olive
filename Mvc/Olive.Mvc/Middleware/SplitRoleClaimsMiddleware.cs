using Microsoft.AspNetCore.Http;
using Olive;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    class SplitRoleClaimsMiddleware
    {
        readonly RequestDelegate Next;

        public SplitRoleClaimsMiddleware(RequestDelegate next) => Next = next;

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
                SplitRoles(context.User);

            await Next.Invoke(context);
        }

        void SplitRoles(ClaimsPrincipal user)
        {
            var splitRoles = user.GetRoles().Trim()
                .Select(x => new Claim(ClaimTypes.Role, x))
                .ToArray();

            if (!(user.Identity is ClaimsIdentity identity)) return;

            var old = identity.Claims.Where(x => x.Type == ClaimTypes.Role).ToArray();
            old.Do(x => identity.RemoveClaim(x));
            identity.AddClaims(splitRoles);
        }
    }
}