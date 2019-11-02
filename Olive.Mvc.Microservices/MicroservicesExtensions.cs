using Microsoft.Extensions.DependencyInjection;

namespace Olive.Mvc.Microservices
{
    public static class MicroservicesExtensions
    {
        public static IServiceCollection AddTSConfiguration(this IServiceCollection services, string relativePath = "scripts/configureServices") =>
            services.AddSingleton(new ServiceConfigurationLocator(relativePath));

        public static IServiceCollection AddEmptyTSConfiguration(this IServiceCollection services) =>
            services.AddSingleton(new ServiceConfigurationLocator(null));
    }
}
