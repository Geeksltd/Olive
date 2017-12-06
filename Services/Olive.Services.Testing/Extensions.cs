using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Services.Testing
{
    public static class TestingExtensions
    {
        public static IApplicationBuilder UseWebTestMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<WebTestMiddleware>();
    }
}
