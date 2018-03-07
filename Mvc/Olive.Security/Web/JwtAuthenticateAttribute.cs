using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Security
{
    public class JwtAuthenticateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var user = context.HttpContext.User;
            if (user == null || user.IsInRole("Anonymous") || user.Claims.None())
            {
                var headerVlaue = context.HttpContext.Request.Headers["Authorization"].ToString();
                if (!headerVlaue.StartsWith("Bearer")) return;
                var jwt = headerVlaue.TrimStart("Bearer").TrimStart();
                if (jwt.IsEmpty()) return;

                try
                {
                    user = OAuth.Instance.DecodeJwt(jwt);
                    if (user != null) Context.Current.Http().User = user;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to decode JWT token: " + ex.Message);
                }
            }
        }
    }
}