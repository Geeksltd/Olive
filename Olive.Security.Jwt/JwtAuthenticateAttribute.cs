using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Security
{
    public class JwtAuthenticateAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            if (User == null || User is WindowsPrincipal || User.IsInRole("Anonymous"))
                await JwtAuthenticate();
        }

        protected async Task JwtAuthenticate()
        {
            HttpRequestHeaders headers = null; // TODO:...
            var user = await JwtAuthentication.ExtractUser(headers);

            if (user != null)
            {
                if (user is IPrincipal principal)
                {
                    Context.Http.User = new System.Security.Claims.ClaimsPrincipal(principal);
                }
                else
                {
                    throw new Exception("User should implement IPrincipal.");
                }
            }
        }
    }
}
