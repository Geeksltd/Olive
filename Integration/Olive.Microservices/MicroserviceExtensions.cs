using System;

namespace Olive
{
    public static class MicroserviceExtensions
    {
        /// <summary>Adds a configuration for the ApiClient instance used to invoke this Api.</summary>
        public static T Use<T>(this T proxy, Action<ApiClient> config) where T : StronglyTypedApiProxy
            => (T)proxy.Configure(config);

        /// <summary>
        /// Sets the cache choice for Get requests made by this proxy.
        /// </summary>
        public static T Cache<T>(this T proxy, CachePolicy policy, TimeSpan? cacheExpiry = null)
            where T : StronglyTypedApiProxy
        {
            proxy.Configure(x => x.Cache(policy, cacheExpiry));
            return proxy;
        }
    }
}