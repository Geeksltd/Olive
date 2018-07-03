using System;
using System.Collections.Generic;

namespace Olive.Mvc
{
    public class JavascriptDependency
    {
        protected List<JavascriptDependency> Dependencies = new List<JavascriptDependency>();

        public string Url { get; private set; }

        protected JavascriptDependency(string url) => Url = url;

        /// <summary>Creates a javascript dependency that will be referenced as a relative path on the client side.
        /// So the final address will be resolved relative to the browser's current url.</summary>
        /// <param name="relativePath">The relative path of the javascript module or library inside wwwroot.
        /// The .js extension is optional.
        /// E.g. /scripts/CustomModule1 or /lib/jquery/jquery.js</param>
        public static JavascriptDependency Relative(string relativePath)
        {
            return new JavascriptDependency(GetFullUrl(relativePath));
        }

        /// <summary>Creates a javascript dependency that will be referenced as an absolute path on the client side.</summary>
        /// <param name="relativeOrAbsolutePath">If an absolute url is provided (i.e. starting with //, http:// or https://, then it will be left as is.
        /// If it's relative, the file will be expected to be inside wwwroot, and its absolute url (based on the current http application's root url) will be referenced.
        /// This is useful for micro-services that are hosted inside a UI container.
        /// The .js extension is optional.
        /// E.g. /scripts/CustomModule1 or http://some-domain.com/lib/jquery/jquery.js</param>
        public static JavascriptDependency Absolute(string relativeOrAbsolutePath)
        {
            return new JavascriptDependency(GetFullUrl(relativeOrAbsolutePath, absolute: true));
        }

        internal static string GetFullUrl(string path, bool absolute = false)
        {
            if (path.IsEmpty()) throw new ArgumentNullException(nameof(path));

            if (!absolute && path.StartsWithAny("http://", "https://", "//"))
                throw new ArgumentException("An absolute javascript url path cannot be sent as relativePath.");

            if (path.StartsWithAny("http://", "https://", "//")) return path;
            else if (absolute)
                return Context.Current.Request().GetAbsoluteUrl(path);
            else
                return path.EnsureStartsWith("/");
        }

        public JavascriptDependency Add(JavascriptDependency dependency)
        {
            Dependencies.Add(dependency ?? throw new ArgumentNullException(nameof(dependency)));
            return this;
        }
    }
}