using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Olive;

namespace Controllers
{
    public class BaseController : Olive.Mvc.Controller
    {
        public BaseController()
        {
            ApiClient.FallBack.Handle(arg => Notify(arg.FriendlyMessage, false));
        }

        [NonAction]
        public new ActionResult Unauthorized() => Redirect("/login");

        protected override string GetDefaultBrowserTitle(ActionExecutingContext context)
            => Microservice.Me.Name + " > " + base.GetDefaultBrowserTitle(context);
    }
}

namespace ViewComponents
{
    public abstract class ViewComponent : Olive.Mvc.ViewComponent { }
}