using System;
using System.Threading.Tasks;

namespace Olive
{
    public interface IGetImplementation<TResponse>
    {
        TResponse Result { get; set; }

        Task<bool> Attempt(ApiClient apiClient, string url, TimeSpan? cacheAge, FallBackEventPolicy fallBackEventPolicy);
    }
}