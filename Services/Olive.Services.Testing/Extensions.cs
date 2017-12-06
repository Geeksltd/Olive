using Microsoft.AspNetCore.Builder;

namespace Olive.Services.Testing
{
    public static class TestingExtensions
    {
        public static IApplicationBuilder UseWebTestMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<WebTestMiddleware>();
    }
}
