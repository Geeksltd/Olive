using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Olive.Entities.Data
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection @this, IConfiguration config)
        {
            @this.AddSingleton<IDatabaseProviderConfig, DatabaseProviderConfig>();
            @this.AddTransient<ICacheProvider, InMemoryCacheProvider>();
            @this.AddTransient<ICache, Cache>();

            if (config["Database:Cache:Mode"] == "multi-server") @this.AddScoped<IDatabase, Database>();
            else @this.AddSingleton<IDatabase, Database>();

            return @this;
        }
    }
}
