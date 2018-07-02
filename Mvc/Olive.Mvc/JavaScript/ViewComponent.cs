using System;

namespace Olive.Mvc
{
    partial class ViewComponent
    {
        /// <summary>
        /// Loads a Javascript (or Typescript) module upon page startup.
        /// </summary>
        /// <param name="relativePath">The relative path of the module inside wwwroot (including the .js extension).
        /// E.g. /scripts/CustomModule1</param>
        /// <param name="staticFunctionInvokation">An expression to call [a static method] on the loaded module.</param>
        [Obsolete("Instead, use JavaScript(new JavaScriptModule(...)).")]
        public virtual void LoadJavascriptModule(string relativePath, string staticFunctionInvokation = "run()")
        {
            JavaScript(new JavascriptModule(relativePath), staticFunctionInvokation);
        }

        /// <summary>
        /// Loads a Javascript (or Typescript) module upon page startup.
        /// </summary>        
        /// <param name="staticFunctionInvokation">An expression to call [a static method] on the loaded module.</param>
        public void JavaScript(JavascriptModule module, string staticFunctionInvokation = "run()")
        {
            Context.Current.Http().JavascriptActions()
              .JavaScript(module.GenerateLoad(staticFunctionInvokation));
        }
    }
}