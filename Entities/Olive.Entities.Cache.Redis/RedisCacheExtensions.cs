using Microsoft.Extensions.DependencyInjection;
namespace Olive.Entities.Data
{
    public static class RedisCacheExtensions
    {
        public static void AddRedisCache(this IServiceCollection @this)
        {
            @this.Replace<ICacheProvider, RedisCacheProvider>(ServiceLifetime.Singleton);
        }
    }
}
