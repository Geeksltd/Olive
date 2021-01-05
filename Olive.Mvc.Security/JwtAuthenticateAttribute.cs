using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics;
using System.Security.Claims;

namespace Olive.Security
{
    public class JwtAuthenticateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!NeedsAuthenticating(context.HttpContext.User))
                return;

            var headerVlaue = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (!headerVlaue.StartsWith("Bearer")) return;
            var jwt = headerVlaue.TrimStart("Bearer").TrimStart();
            if (jwt.IsEmpty()) return;

            try
            {
                var user = OAuth.Instance.DecodeJwt(jwt);
                if (user != null) Context.Current.Http().User = user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to decode JWT token: " + ex.Message);
            }
        }

        bool NeedsAuthenticating(ClaimsPrincipal user)
        {
            if (user == null) return true;
            if (user.IsInRole("Anonymous")) return true;

            if (user.Claims
                .Except(x => x.Type == ClaimTypes.Role && x.Value == "Local.Request")
                .None())
                return true;

            return false;
        }
    }
}
