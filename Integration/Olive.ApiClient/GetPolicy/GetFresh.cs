using System;
using System.Threading.Tasks;
using static Olive.ApiClient;

namespace Olive
{
    class GetFresh<T> : GetImplementation<T>
    {
        public GetFresh(ApiClient client) : base(client) { }

        public override async Task<bool> Attempt(string url)
        {
            try
            {
                var cache = ApiResponseCache<T>.Create(url);

                var requestInfo = new RequestInfo(ApiClient) { HttpMethod = "GET" };

                Result = await requestInfo.TrySend<T>();

                if (requestInfo.Error == null)
                {
                    await cache.File.WriteAllTextAsync(requestInfo.ResponseText);

                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.For(this).Error(exception);
            }

            return false;
        }
    }
}