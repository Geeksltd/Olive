using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public Task<TResponse> Patch<TResponse, TEntity>(TEntity entity, string requestData)
            where TEntity : IQueueable<Guid>
        {
            return Patch<TResponse, TEntity, Guid>(entity, requestData);
        }

        public Task<TResponse> Patch<TResponse, TEntity>(
          TEntity entity, object jsonParams) where TEntity : IQueueable<Guid>
        {
            return Patch<TResponse, TEntity, Guid>(entity, jsonParams);
        }

        public async Task<TResponse> Patch<TResponse, TEntity, TIdentifier>(
            TEntity entity, object jsonParams) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPatch<TResponse, TEntity, TIdentifier>(entity, null, jsonParams)).Item1;
        }

        public async Task<TResponse> Patch<TResponse, TEntity, TIdentifier>(
          TEntity entity, string requestData) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPatch<TResponse, TEntity, TIdentifier>(entity, requestData, null)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoPatch<TResponse, TEntity, TIdentifier>(
       TEntity entity, string requestData, object jsonParams) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(this)
            {
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