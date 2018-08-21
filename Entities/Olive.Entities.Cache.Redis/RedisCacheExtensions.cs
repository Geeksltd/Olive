using Microsoft.Extensions.DependencyInjection;

namespace Olive.Entities.Data
{
    public static class RedisCacheExtensions
    {
        public static void AddRedisCache(this IServiceCollection @this, bool clearOnStart = true)
        {
            Cache.Instance = new RedisCache();
            if (clearOnStart) Cache.Instance.ClearAll();
        }
    }
}
