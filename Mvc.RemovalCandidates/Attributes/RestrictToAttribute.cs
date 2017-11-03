using System;
using System.Collections.Generic;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class RestrictToAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (HttpContext.Current.Request.IsAjaxCall())
            {
                var actions = new List<object>
                {
                    new { OutOfModal = true, WithAjax = false, Redirect = Config.Get("Authentication.LoginUrl") }
                };

                filterContext.Result = new JsonResult(actions);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}