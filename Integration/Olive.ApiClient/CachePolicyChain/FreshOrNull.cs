using System;
using System.Threading.Tasks;

namespace Olive
{
    public class FreshOrNull<T> : GetResponse<T>
    {
        private readonly CachePolicy _cachePolicy;

        public FreshOrNull(ApiClient apiClient, string url, CachePolicy cachePolicy) : base(apiClient, url)
        {
            _cachePolicy = cachePolicy;
        }

        public override Task<T> Execute()
        {
            if (_cachePolicy == CachePolicy.FreshOrNull)
            {
                try
                {
                    return ExecuteGet();
                }
                catch (Exception e)
                {
                    Log.For(this).Error(e);

                    return null;
                }
            }

            return base.Execute();
        }
    }
}