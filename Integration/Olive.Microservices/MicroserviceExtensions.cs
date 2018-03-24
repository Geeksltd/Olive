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

        /// <summary>
        /// Prevents sending too many requests to an already failed remote service.
        /// If http exceptions are raised consecutively for the specified number of times,
        /// it will 'break the circuit' for the specified duration.
        /// During the break period, any attempt to execute a new request will immediately
        /// throw a BrokenCircuitException. Once the duration is over, if the first action
        /// throws http exception again, the circuit will break again for the same duration.
        /// Otherwise the circuit will reset.
        /// </summary>
        public static T CircuitBreaker<T>(this T proxy, int exceptionsBeforeBreaking = 5, int breakDurationSeconds = 10)
        where T : StronglyTypedApiProxy
        {
            proxy.Configure(x => x.CircuitBreaker(exceptionsBeforeBreaking, breakDurationSeconds));
            return proxy;
        }

        /// <summary>
        /// Sets the number of retries before giving up. Default is zero.
        /// </summary>
        public static T Retries<T>(this T proxy, int retries, int pauseMilliseconds = 100)
        where T : StronglyTypedApiProxy
        {
            proxy.Configure(x => x.Retries(retries, pauseMilliseconds));
            return proxy;
        }
    }
}