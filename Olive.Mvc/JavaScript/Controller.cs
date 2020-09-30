using System;
using Microsoft.AspNetCore.Mvc;

namespace Olive.Mvc
{
    partial class Controller
    {
        JavascriptActions JavascriptActions => HttpContext.JavascriptActions();

        [NonAction]
        public JsonResult JavaScript(string script, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            JavascriptActions.JavaScript(script, stage);
            return JsonActions();
        }

        /// <summary>
        /// Loads a Javascript (or Typescript) module upon page startup.
        /// </summary>        
        /// <param name="staticFunctionInvokation">An expression to call [a static method] on the loaded module.</param>
        [NonAction]
        public JsonResult JavaScript(JavascriptModule module, string staticFunctionInvokation = "run()")
        {
            return JavaScript(module.GenerateLoad(staticFunctionInvokation));
        }

        /// <summary>
        /// Loads a Javascript (or Typescript) service module upon page startup.
        /// </summary>
        [NonAction]
        public virtual JsonResult JavaScript(JavascriptService service)
        {
            JavascriptActions.Add(service);
            return JsonActions();
        }

        /// <param name="javascriptCode">The code to run after a set of javascritp dependencies are loaded.</param>
        [NonAction]
        public JsonResult JavaScript(JavascriptDependency[] dependencies, string javascriptCode)
        {
            return JavaScript(dependencies.OnLoaded(javascriptCode));
        }

        [NonAction, Obsolete("Instead, use JavaScript(new JavaScriptModule(...)).", error: true)]
        public virtual void LoadJavascriptModule(string relativePath, string staticFunctionInvokation = "run()")
        {
            JavaScript(JavascriptModule.Relative(relativePath), staticFunctionInvokation);
        }
    }
}