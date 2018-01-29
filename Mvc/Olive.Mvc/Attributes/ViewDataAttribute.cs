using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class ViewDataAttribute : ActionFilterAttribute
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public ViewDataAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ((Controller)context.Controller).ViewData[Key] = Value;
            base.OnActionExecuting(context);
        }
    }
}