using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{
    class ServiceConfigurationLocator
    {
        readonly string RelativePath;

        public ServiceConfigurationLocator(string relativePath) => RelativePath = relativePath.TrimEnd(".ts", caseSensitive: false).EnsureEndsWith(".js");

        public bool HasConfiguration => RelativePath.HasValue();
        public string GetUrl(HttpContext context) => context.Request.GetAbsoluteUrl(RelativePath);
    }
}
