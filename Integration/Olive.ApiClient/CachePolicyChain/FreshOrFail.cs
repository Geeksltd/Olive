using System.Threading.Tasks;

namespace Olive
{
    public class FreshOrFail<T> : GetResponse<T>
    {
        private readonly CachePolicy _cachePolicy;

        public FreshOrFail(ApiClient apiClient, string url, CachePolicy cachePolicy) : base(apiClient, url)
        {
            _cachePolicy = cachePolicy;
        }

        public override Task<T> Execute() => _cachePolicy == CachePolicy.FreshOrFail ? ExecuteGet() : base.Execute();
    }
}