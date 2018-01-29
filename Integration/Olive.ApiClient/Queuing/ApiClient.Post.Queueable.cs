using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public Task<TResponse> Post<TResponse, TEntity>(TEntity entity, string requestData) where TEntity : IQueueable<Guid>
        {
            return Post<TResponse, TEntity, Guid>(entity, requestData);
        }

        public async Task<TResponse> Post<TResponse, TEntity, TIdentifier>(
           TEntity entity, string requestData) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPost<TResponse, TEntity, TIdentifier>(entity, requestData, null)).Item1;
        }

        public Task<TResponse> Post<TResponse, TEntity>(
           TEntity entity, object jsonParams) where TEntity : IQueueable<Guid>
        {
            return Post<TResponse, TEntity, Guid>(entity, jsonParams);
        }

        public async Task<TResponse> Post<TResponse, TEntity, TIdentifier>(
            TEntity entity, object jsonParams) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPost<TResponse, TEntity, TIdentifier>(entity, null, jsonParams)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoPost<TResponse, TEntity, TIdentifier>(
       TEntity entity, string requestData, object jsonParams) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(this)
            {
                HttpMethod = "POST",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send<TEntity, TIdentifier>(entity)) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}