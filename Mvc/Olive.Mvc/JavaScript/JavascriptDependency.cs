using System;
using System.Collections.Generic;

namespace Olive.Mvc
{
    public class JavascriptDependency
    {
        protected List<JavascriptDependency> Dependencies = new List<JavascriptDependency>();

        public string RelativePath { get; }

        /// <param name="relativePath">The relative path of the javascript module or library inside wwwroot.
        /// The .js extension is optional.
        /// E.g. /scripts/CustomModule1 or /lib/jquery/jquery.js</param>
        public JavascriptDependency(string relativePath)
        {
            RelativePath = relativePath;
        }

        internal string GetFullUrl()
        {
            string fullUrl;

            var isAbsolute = RelativePath.StartsWithAny("http://", "https://", "//");
            if (isAbsolute) fullUrl = RelativePath;
            else fullUrl = RelativePath.EnsureStartsWith("/");

            return fullUrl;
        }

        public JavascriptDependency Add(JavascriptDependency dependency)
        {
            Dependencies.Add(dependency ?? throw new ArgumentNullException(nameof(dependency)));
            return this;
        }

        /// <param name="relativePath">The relative path of the javascript module or library inside wwwroot.
        /// The .js extension is optional. For example: /lib/jquery/jquery.js</param>
        public JavascriptDependency Add(string relativePath)
            => Add(new JavascriptDependency(relativePath));
    }
}
