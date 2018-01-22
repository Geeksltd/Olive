using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public Task<TResponse> Delete<TResponse, TEntity>(TEntity entity, object jsonParams) where TEntity : IQueueable<Guid>
        {
            return Delete<TResponse, TEntity, Guid>(entity, jsonParams);
        }

        public async Task<TResponse> Delete<TResponse, TEntity, TIdentifier>(
            TEntity entity, object jsonParams) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoDelete<TResponse, TEntity, TIdentifier>(entity, jsonParams)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoDelete<TResponse, TEntity, TIdentifier>(
        TEntity entity, object jsonParams) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(this)
            {
                HttpMethod = "DELETE",
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send<TEntity, TIdentifier>(entity)) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}