using System;
using System.Threading.Tasks;

namespace Olive
{
    public class CacheOrFail<T> : GetResponse<T>
    {
        private readonly string _url;
        private readonly CachePolicy _cachePolicy;
        private readonly TimeSpan? _cacheExpiry;

        public CacheOrFail(ApiClient apiClient, string url, CachePolicy cachePolicy, TimeSpan? cacheExpiry) : base(apiClient, url)
        {
            _url = url;
            _cachePolicy = cachePolicy;
            _cacheExpiry = cacheExpiry;
        }

        public override async Task<T> Execute()
        {
            if (_cachePolicy == CachePolicy.CacheOrFail)
            {
                var cache = ApiResponseCache<T>.Create(_url);

                if (await cache.HasValidValue(_cacheExpiry))
                    return cache.Data;

                throw new NullReferenceException("Nothing has been cached!");
            }

            return await base.Execute();
        }
    }
}