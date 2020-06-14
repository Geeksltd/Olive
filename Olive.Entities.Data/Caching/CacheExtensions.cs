using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Olive.Entities.Data
{
    public static class CacheExtensions
    {
        public static bool IsCacheable(this Type type)
            => CacheObjectsAttribute.IsEnabled(type) ?? Database.Configuration.Cache.Enabled;

        public static IServiceCollection AddDatabase(this IServiceCollection @this, IConfiguration config)
        {
            @this.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

            if (config["Database:Cache:PerRequest"] == "True") @this.AddScoped<ICache, Cache>();
            else @this.AddSingleton<ICache, Cache>();

            @this.AddSingleton<IDatabase, Database>();

            return @this;
        }

        public static ICache Cache(this IDatabase database) => Context.Current.Cache();
    }
}
