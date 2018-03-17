using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace Olive.Mvc
{
    public class AuthorizeApiAttribute : TypeFilterAttribute
    {
        public AuthorizeApiAttribute(string claimType, string claimValue) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new Claim(claimType, claimValue) };
        }
    }
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;
        public string Role { get; set; }

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
                context.Result = new StatusCodeResult(401);
            else if (!context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value))
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
