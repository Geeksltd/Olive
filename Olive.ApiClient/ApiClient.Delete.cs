using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public async Task<bool> Delete(object jsonParams = null)
        {
            var result = await DoDelete<string>(jsonParams).ConfigureAwait(false);
            return result.Item2.Error == null;
        }

        public async Task<TResponse> Delete<TResponse>(object jsonParams)
        {
            return (await DoDelete<TResponse>(jsonParams)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoDelete<TResponse>(object jsonParams)
        {
            var request = new RequestInfo(this)
            {
                HttpMethod = "DELETE",
                JsonData = jsonParams
            };

            return Tuple.Create(await request.TrySend<TResponse>(), request);
        }
    }
}