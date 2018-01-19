using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public static Task<TResponse> Delete<TResponse, TEntity>(
              TEntity entity,
              string url,
              object jsonParams,
              OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<Guid>
        {
            return Delete<TResponse, TEntity, Guid>(entity, url, jsonParams, errorAction);
        }

        public static async Task<TResponse> Delete<TResponse, TEntity, TIdentifier>(
            TEntity entity,
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoDelete<TResponse, TEntity, TIdentifier>(entity, url, jsonParams, errorAction)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoDelete<TResponse, TEntity, TIdentifier>(
        TEntity entity,
        string url,
        object jsonParams,
        OnApiCallError errorAction) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(url)
            {
                ErrorAction = errorAction,
                HttpMethod = "DELETE",
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send<TEntity, TIdentifier>(entity)) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}