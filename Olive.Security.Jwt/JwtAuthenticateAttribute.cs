using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Olive.Web;

namespace Olive.Security
{
    public class JwtAuthenticateAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            var user = context.HttpContext.User;

            if (user == null || user is WindowsPrincipal || user.IsInRole("Anonymous"))
            {
                // No user from Cookie. Use the header:
                var jwtUser = JwtAuthentication.ExtractUser(context.HttpContext.Request.Headers);
                if (jwtUser != null) Context.Http.User = jwtUser;
            }
        }
    }
}