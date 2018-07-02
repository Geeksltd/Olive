using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Olive.Entities;

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

        /// <param name="javascriptCode">The code to run after a set of javascritp dependencies are loaded.</param>
        [NonAction]
        public JsonResult JavaScript(string[] dependencies, string javascriptCode)
        {
            return JavaScript(dependencies.Select(x => new JavascriptDependency(x)).OnLoaded(javascriptCode));
        }

        /// <param name="javascriptCode">The code to run after a set of javascritp dependencies are loaded.</param>
        [NonAction]
        public JsonResult JavaScript(JavascriptDependency[] dependencies, string javascriptCode)
        {
            return JavaScript(dependencies.OnLoaded(javascriptCode));
        }

        [Obsolete("Instead, use JavaScript(new JavaScriptModule(...)).")]
        public virtual void LoadJavascriptModule(string relativePath, string staticFunctionInvokation = "run()")
        {
            JavaScript(new JavascriptModule(relativePath), staticFunctionInvokation);
        }
    }
}