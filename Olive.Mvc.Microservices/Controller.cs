using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Mvc.Microservices
{
    public abstract class Controller : Olive.Mvc.Controller
    {
        protected Controller()
        {
            ApiClient.FallBack += ev => Notify(ev.Args.FriendlyMessage, false);
        }

        protected override bool IsMicrofrontEnd => true;

        protected override string GetDefaultBrowserTitle(ActionExecutingContext context)
            => Microservice.Me.Name + " > " + base.GetDefaultBrowserTitle(context);

        public override JsonResult JavaScript(JavascriptService service)
        {
            var locator = Context.Current.GetOptionalService<ServiceConfigurationLocator>();

            if (locator == null || !locator.HasConfiguration)
                return base.JavaScript(service);

            return base.JavaScript(new MicroserviceJavascriptService(locator.GetUrl(HttpContext), service));
        }
    }
}