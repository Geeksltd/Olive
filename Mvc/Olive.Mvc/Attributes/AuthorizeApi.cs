using System.ComponentModel;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Olive.Mvc
{
    public class AuthorizeApiAttribute : TypeFilterAttribute
    {
        string roles;

        public AuthorizeApiAttribute(string roles = null) : base(typeof(ClaimRequirementFilter))
        {
            Roles = roles;
        }

        /// <summary>
        /// A comma separated list of roles.
        /// </summary>
        public string Roles
        {
            get => roles;
            set
            {
                roles = value;
                Arguments = new object[] { value };
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly string[] Roles;
        ILogger Log;

        public ClaimRequirementFilter(string roles)
        {
            Roles = roles.OrEmpty().Split(',').Trim().ToArray();
            Log = Olive.Log.For(this);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            void disallow()
                => context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);

            if (!user.Identity.IsAuthenticated)
            {
                Log.Debug("OnAuthorization: user.Identity.IsAuthenticated is FALSE");
                disallow();
            }
            else if (Roles.Any() && Roles.None(x => user.IsInRole(x)))
            {
                Log.Debug("OnAuthorization: User does not have any role: " + Roles.ToString(", "));
                disallow();
            }
        }
    }
}