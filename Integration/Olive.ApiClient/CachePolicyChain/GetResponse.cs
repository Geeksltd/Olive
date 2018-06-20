using System.Threading.Tasks;
using static Olive.ApiClient;

namespace Olive
{
    public abstract class GetResponse<TResponse>
    {
        private readonly ApiClient _apiClient;
        private readonly string _url;
        private GetResponse<TResponse> _nextGetResponse;

        protected GetResponse(ApiClient apiClient, string url)
        {
            _apiClient = apiClient;
            _url = url;
        }

        public void SetSuccessor(GetResponse<TResponse> getResponse)
        {
            _nextGetResponse = getResponse;
        }

        /// <summary>
        ///     Return data from cache or make a new HTTP request
        /// </summary>
        /// <returns>Response</returns>
        public virtual async Task<TResponse> Execute()
        {
            if (_nextGetResponse != null) return await _nextGetResponse.Execute();

            return await ExecuteGet();
        }

        /// <summary>
        ///     Send request to the server and create its cache file
        /// </summary>
        /// <returns>Response</returns>
        protected async Task<TResponse> ExecuteGet()
        {
            var cache = ApiResponseCache<TResponse>.Create(_url);

            var request = new RequestInfo(_apiClient) { HttpMethod = "GET" };

            var result = await request.TrySend<TResponse>();

            if (request.Error == null)
            {
                await cache.File.WriteAllTextAsync(request.ResponseText);
                return result;
            }

            throw request.Error;
        }
    }
}