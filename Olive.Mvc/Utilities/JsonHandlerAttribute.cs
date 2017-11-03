using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Mvc
{
    public class JsonHandlerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is JsonResult jsonResult)
                filterContext.Result = new JsonNetResult(jsonResult.Value, jsonResult.SerializerSettings);

            base.OnActionExecuted(filterContext);
        }
    }
}
