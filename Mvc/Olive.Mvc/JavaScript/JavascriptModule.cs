namespace Olive.Mvc
{
    public class JavascriptModule : JavascriptDependency
    {
        JavascriptModule(string url) : base(url) { }

        /// <summary>Creates a javascript module reference that will be referenced as a relative path on the client side.
        /// The final address will be resolved relative to the browser's current url.</summary>
        /// <param name="relativePath">The relative path of the javascript module or library inside wwwroot.
        /// The .js extension is optional.
        /// E.g. /scripts/CustomModule1 or /lib/jquery/jquery.js</param>
        public static new JavascriptModule Relative(string relativePath)
        {
            return new JavascriptModule(GetFullUrl(relativePath));
        }

        /// <summary>Creates a javascript module reference that will be referenced as an absolute path on the client side.</summary>
        /// <param name="relativeOrAbsolutePath">If an absolute url is provided (i.e. starting with //, http:// or https://, then it will be left as is.
        /// If it's relative, the file will be expected to be inside wwwroot, and its absolute url (based on the current http application's root url) will be referenced.
        /// This is useful for micro-services that are hosted inside a UI container.
        /// The .js extension is optional.
        /// E.g. /scripts/CustomModule1 or http://some-domain.com/lib/jquery/jquery.js</param>
        public static new JavascriptModule Absolute(string relativeOrAbsolutePath)
        {
            return new JavascriptModule(GetFullUrl(relativeOrAbsolutePath, absolute: true));
        }

        public string GenerateLoad(string staticFunctionInvokation = "run()")
        {
            var onLoaded = staticFunctionInvokation.WithPrefix(", m => m.default.");

            var result = $"requirejs(['{Url}']{onLoaded});";

            return Dependencies.OnLoaded(result);
        }

        public new JavascriptModule Add(JavascriptDependency dependency)
        {
            return (JavascriptModule)base.Add(dependency);
        }
    }
}