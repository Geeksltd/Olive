using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public static Task<TResponse> Put<TResponse, TEntity>(
          TEntity entity,
          string url,
          string requestData,
          OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<Guid>
        {
            return Put<TResponse, TEntity, Guid>(entity, url, requestData, errorAction);
        }

        public static async Task<TResponse> Put<TResponse, TEntity, TIdentifier>(
           TEntity entity,
           string url,
           string requestData,
           OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPut<TResponse, TEntity, TIdentifier>(entity, url, requestData, null, errorAction)).Item1;
        }

        public static Task<TResponse> Put<TResponse, TEntity>(
        TEntity entity,
        string url,
        object jsonParams,
        OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<Guid>
        {
            return Put<TResponse, TEntity, Guid>(entity, url, jsonParams, errorAction);
        }

        public static async Task<TResponse> Put<TResponse, TEntity, TIdentifier>(
          TEntity entity,
          string url,
          object jsonParams,
          OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPut<TResponse, TEntity, TIdentifier>(entity, url, null, jsonParams, errorAction)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoPut<TResponse, TEntity, TIdentifier>(
        TEntity entity,
        string url,
        string requestData,
        object jsonParams,
        OnApiCallError errorAction) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(url)
            {
                ErrorAction = errorAction,
                HttpMethod = "PUT",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send<TEntity, TIdentifier>(entity)) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}