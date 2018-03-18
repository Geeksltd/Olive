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
        readonly Claim claim;
        public string Role { get; set; }

        public ClaimRequirementFilter(Claim claim) => this.claim = claim;
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
                context.Result = new StatusCodeResult(401);
            else if (!context.HttpContext.User.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
