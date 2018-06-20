using System;
using System.Threading.Tasks;

namespace Olive
{
    public class CacheOrFreshOrNull<T> : GetResponse<T>
    {
        private readonly TimeSpan? _cacheExpiry;
        private readonly CachePolicy _cachePolicy;
        private readonly string _url;

        public CacheOrFreshOrNull(ApiClient apiClient, string url, CachePolicy cachePolicy, TimeSpan? cacheExpiry) :
            base(apiClient, url)
        {
            _url = url;
            _cachePolicy = cachePolicy;
            _cacheExpiry = cacheExpiry;
        }

        public override async Task<T> Execute()
        {
            if (_cachePolicy == CachePolicy.CacheOrFreshOrNull)
            {
                var cache = ApiResponseCache<T>.Create(_url);

                if (await cache.HasValidValue(_cacheExpiry))
                    return cache.Data;

                try
                {
                    return await ExecuteGet();
                }
                catch (Exception exception)
                {
                    Log.For(this).Error(exception);

                    return default(T);
                }
            }

            return await base.Execute();
        }
    }
}