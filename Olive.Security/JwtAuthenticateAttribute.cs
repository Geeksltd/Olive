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
                var headerVlaue = context.HttpContext.Request.Headers["Authorization"].ToString();
                if (!headerVlaue.StartsWith("Bearer")) return;
                var jwt = headerVlaue.TrimStart("Bearer").TrimStart();
                if (jwt.IsEmpty()) return;

                try
                {
                    user = OAuth.DecodeJwt(jwt);
                    if (user != null) Context.Http.User = user;
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}