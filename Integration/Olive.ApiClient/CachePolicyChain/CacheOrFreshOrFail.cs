using System;
using System.Threading.Tasks;

namespace Olive
{
    public class CacheOrFreshOrFail<T> : GetResponse<T>
    {
        private readonly TimeSpan? _cacheExpiry;
        private readonly CachePolicy _cachePolicy;
        private readonly string _url;

        public CacheOrFreshOrFail(ApiClient apiClient, string url, CachePolicy cachePolicy, TimeSpan? cacheExpiry) :
            base(apiClient, url)
        {
            _url = url;
            _cachePolicy = cachePolicy;
            _cacheExpiry = cacheExpiry;
        }

        public override async Task<T> Execute()
        {
            if (_cachePolicy == CachePolicy.CacheOrFreshOrFail)
            {
                var cache = ApiResponseCache<T>.Create(_url);

                if (await cache.HasValidValue(_cacheExpiry))
                    return cache.Data;

                return await ExecuteGet();
            }

            return await base.Execute();
        }
    }
}