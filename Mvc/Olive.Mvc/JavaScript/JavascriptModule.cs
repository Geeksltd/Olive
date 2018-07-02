using System.Linq;

namespace Olive.Mvc
{
    public class JavascriptModule : JavascriptDependency
    {
        /// <param name="relativePath">The relative path of the module inside wwwroot (including the .js extension).
        /// E.g. /scripts/CustomModule1</param>
        public JavascriptModule(string relativePath) : base(relativePath) { }

        public string GenerateLoad(string staticFunctionInvokation = "run()")
        {
            var onLoaded = staticFunctionInvokation.WithPrefix(", m => m.default.");

            var result = $"requirejs(['{GetFullUrl()}']{onLoaded});";

            return Dependencies.OnLoaded(result);
        }

        public new JavascriptModule Add(JavascriptDependency dependency)
        {
            return (JavascriptModule)base.Add(dependency);
        }

        /// <param name="relativePath">The relative path of the javascript module or library inside wwwroot.
        /// The .js extension is optional. For example: /lib/jquery/jquery.js</param>
        public new JavascriptModule Add(string relativePath)
            => Add(new JavascriptDependency(relativePath));
    }
}
