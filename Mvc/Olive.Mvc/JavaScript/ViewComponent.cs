using System;

namespace Olive.Mvc
{
    partial class ViewComponent
    {
        JavascriptActions JavascriptActions => Context.Current.Http().JavascriptActions();

        /// <summary>
        /// Loads a Javascript (or Typescript) module upon page startup.
        /// </summary>
        /// <param name="relativePath">The relative path of the module inside wwwroot (including the .js extension).
        /// E.g. /scripts/CustomModule1</param>
        /// <param name="staticFunctionInvokation">An expression to call [a static method] on the loaded module.</param>
        [Obsolete("Instead, use JavaScript(new JavaScriptModule(...)).", error: true)]
        public virtual void LoadJavascriptModule(string relativePath, string staticFunctionInvokation = "run()")
        {
            JavaScript(JavascriptModule.Relative(relativePath), staticFunctionInvokation);
        }

        /// <summary>
        /// Loads a Javascript (or Typescript) module upon page startup.
        /// </summary>        
        /// <param name="staticFunctionInvokation">An expression to call [a static method] on the loaded module.</param>
        public void JavaScript(JavascriptModule module, string staticFunctionInvokation = "run()")
        {
            JavascriptActions.JavaScript(module.GenerateLoad(staticFunctionInvokation));
        }

        public void JavaScript(string script, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            JavascriptActions.JavaScript(script, stage);
        }

        /// <param name="javascriptCode">The code to run after a set of javascritp dependencies are loaded.</param>       
        public void JavaScript(JavascriptDependency[] dependencies, string javascriptCode)
        {
            JavaScript(dependencies.OnLoaded(javascriptCode));
        }
    }
}