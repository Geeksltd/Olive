using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Mvc
{
    public class AuthorizeApiAttribute : TypeFilterAttribute
    {
        public AuthorizeApiAttribute() : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[0];
        }

        /// <summary>
        /// A comma separated list of roles.
        /// </summary>
        public string Roles
        {
            set => Arguments = value.OrEmpty().Split(',').Trim().ToArray();
        }
    }

    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly string[] Roles;

        public ClaimRequirementFilter(object[] roles) => Roles = roles.Select(x => x.ToString()).ToArray();

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            var allowed = user.Identity.IsAuthenticated;

            if (Roles.Any() && Roles.None(x => user.IsInRole(x)))
                allowed = false;

            if (!allowed)
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
    }
}