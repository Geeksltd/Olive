using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Mvc.Microservices
{
    public abstract class Controller : Olive.Mvc.Controller
    {
        public Controller()
        {
            ApiClient.FallBack.Handle(arg => Notify(arg.FriendlyMessage, false));
        }

        protected override bool IsMicrofrontEnd => true;

        protected override string GetDefaultBrowserTitle(ActionExecutingContext context)
            => Microservice.Me.Name + " > " + base.GetDefaultBrowserTitle(context);
    }
}