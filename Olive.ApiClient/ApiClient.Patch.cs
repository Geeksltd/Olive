using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public async Task<bool> Patch()
        {
            var result = await DoPatch<string>(null, null);
            return result.Item2.Error == null;
        }

        public async Task<bool> Patch(string requestData)
        {
            var result = await DoPatch<string>(requestData, null);
            return result.Item2.Error == null;
        }

        public async Task<bool> Patch(object jsonParams)
        {
            var result = await DoPatch<string>(null, jsonParams);
            return result.Item2.Error == null;
        }

        public async Task<TResponse> Patch<TResponse>(string requestData)
        {
            return (await DoPatch<TResponse>(requestData, null)).Item1;
        }

        public async Task<TResponse> Patch<TResponse>(object jsonParams)
        {
            return (await DoPatch<TResponse>(null, jsonParams)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoPatch<TResponse>(

         string requestData,
         object jsonParams)
        {
            var request = new RequestInfo(this)
            {
                HttpMethod = "PATCH",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send()) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}