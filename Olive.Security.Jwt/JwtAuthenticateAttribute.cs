namespace Olive.Security
{
    public class JwtAuthenticateAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            var user = context.HttpContext.User;

            if (user == null || user is WindowsPrincipal || user.IsInRole("Anonymous"))
                await JwtAuthenticate(context);
        }

        protected async Task JwtAuthenticate(ActionExecutingContext context)
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