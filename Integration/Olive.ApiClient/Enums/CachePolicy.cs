namespace Olive
{
    public enum CachePolicy
    {
        /// <summary>
        /// Choose this if you prefer Up-to-date, Minimum crashing, but not fast or resource efficient.
        /// It makes a fresh HTTP request and it waits for the response. If result was successful, cache will be updated. If it fails, but we already have a cached result from before, that will be returned instead of showing an error. This option is safe, is more likely to give you up to date result, but it's not the fastest option as it always waits for a remote call before returning.
        /// </summary>
        FreshOrCacheOrFail,

        /// <summary>
        /// Choose this if you prefer fast, minimum crashing, resource efficient, but not up-to-date.
        /// If a cache is available from before, it will be returned and no fresh request will be sent. This is the fastest option, but the result can be old.
        /// </summary>
        CacheOrFreshOrFail,

        /// <summary>
        /// Choose this if you definitely need up-to-date, but can live with more potential for crashing, resource inefficient and reduced speed.
        /// A fresh request will be sent always. If it fails, an error will be thrown. Any cache will be ignored.
        /// </summary>
        FreshOrFail
    }
}
