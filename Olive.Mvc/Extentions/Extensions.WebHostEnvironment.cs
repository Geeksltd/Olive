using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc
{
    public static class WebHostEnvironmentExtensions
    {
        public static bool IsProduction(this IWebHostEnvironment @this) => @this.Is(Microsoft.Extensions.Hosting.Environments.Production);

        public static bool IsDevelopment(this IWebHostEnvironment @this) => @this.Is(Microsoft.Extensions.Hosting.Environments.Development);

        public static bool Is(this IWebHostEnvironment @this, string name) => @this.EnvironmentName == name;
    }
}
