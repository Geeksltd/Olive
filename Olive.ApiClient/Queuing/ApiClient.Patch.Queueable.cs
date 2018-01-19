using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public static Task<TResponse> Patch<TResponse, TEntity>(
            TEntity entity,
            string url,
            string requestData,
            OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<Guid>
        {
            return Patch<TResponse, TEntity, Guid>(entity, url, requestData, errorAction);
        }

        public static Task<TResponse> Patch<TResponse, TEntity>(
          TEntity entity,
          string url,
          object jsonParams,
          OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<Guid>
        {
            return Patch<TResponse, TEntity, Guid>(entity, url, jsonParams, errorAction);
        }

        public static async Task<TResponse> Patch<TResponse, TEntity, TIdentifier>(
            TEntity entity,
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPatch<TResponse, TEntity, TIdentifier>(entity, url, null, jsonParams, errorAction)).Item1;
        }

        public static async Task<TResponse> Patch<TResponse, TEntity, TIdentifier>(
          TEntity entity,
          string url,
          string requestData,
          OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPatch<TResponse, TEntity, TIdentifier>(entity, url, requestData, null, errorAction)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoPatch<TResponse, TEntity, TIdentifier>(
       TEntity entity,
      string url,
      string requestData,
      object jsonParams,
      OnApiCallError errorAction) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(url)
            {
                ErrorAction = errorAction,
                HttpMethod = "PATCH",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send<TEntity, TIdentifier>(entity)) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}