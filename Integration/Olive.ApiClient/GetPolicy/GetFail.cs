using System;
using System.Threading.Tasks;

namespace Olive
{
    public class GetFail<T> : IGetImplementation<T>
    {
        public T Result { get; set; }
        public Task<bool> Attempt(ApiClient apiClient, string url, TimeSpan? cacheAge, FallBackEventPolicy fallBackEventPolicy)
        {
            throw new Exception("Get request failed.");
        }
    }
}