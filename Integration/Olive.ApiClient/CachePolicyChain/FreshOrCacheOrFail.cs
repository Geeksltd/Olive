using System;
using System.Threading.Tasks;

namespace Olive
{
    public class FreshOrCacheOrFail<T> : GetResponse<T>
    {
        private readonly TimeSpan? _cacheExpiry;
        private readonly FallBack _fallBack;
        private readonly AsyncEvent<UsingCacheInsteadOfFreshEvent> _apiClientEvent;
        private readonly CachePolicy _cachePolicy;
        private readonly string _url;

        public FreshOrCacheOrFail(ApiClient apiClient, string url, CachePolicy cachePolicy, TimeSpan? cacheExpiry,
            FallBack fallBack, AsyncEvent<UsingCacheInsteadOfFreshEvent> apiClientEvent) :
            base(apiClient, url)
        {
            _url = url;
            _cachePolicy = cachePolicy;
            _cacheExpiry = cacheExpiry;
            _fallBack = fallBack;
            _apiClientEvent = apiClientEvent;
        }

        public override async Task<T> Execute()
        {
            if (_cachePolicy == CachePolicy.FreshOrCacheOrFail)
                try
                {
                    return await ExecuteGet();
                }
                catch (Exception e)
                {
                    Log.For(this).Error(e);
                    var cache = ApiResponseCache<T>.Create(_url);

                    Log.For(this).Warning("Going to use cache");

                    if (await cache.HasValidValue(_cacheExpiry))
                    {
                        Log.For(this).Warning("Cache is valid and fire!!!1");
                        if (_fallBack == FallBack.Warn)
                        {
                            var cacheAge = _cacheExpiry.HasValue
                                ? LocalTime.UtcNow.Subtract(_cacheExpiry.Value).DaysInMonth()
                                : 0;
                            await _apiClientEvent.Raise(new UsingCacheInsteadOfFreshEvent
                            {
                                Url = _url,
                                CacheAge = _cacheExpiry,
                                FriendlyMessage = $"Failed to get fresh results from {_url.AsUri().Host} .using the latest cache from {cacheAge} days ago."
                            });
                        }
                        Log.For(this).Warning("Return cache");
                        return cache.Data;
                    }
                    else
                    {
                        throw;
                    }
                }

            return await base.Execute();
        }
    }
}