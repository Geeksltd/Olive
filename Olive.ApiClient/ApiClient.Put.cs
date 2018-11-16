using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public async Task<bool> Put()
        {
            var result = await DoPut<string>(null, null);
            return result.Item2.Error == null;
        }

        public async Task<bool> Put(string requestData)
        {
            var result = await DoPut<string>(requestData, null);
            return result.Item2.Error == null;
        }

        public async Task<bool> Put(object jsonParams)
        {
            var result = await DoPut<string>(null, jsonParams);
            return result.Item2.Error == null;
        }

        public async Task<TResponse> Put<TResponse>(string requestData)
        {
            return (await DoPut<TResponse>(requestData, null)).Item1;
        }

        public async Task<TResponse> Put<TResponse>(object jsonParams)
        {
            return (await DoPut<TResponse>(null, jsonParams)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoPut<TResponse>(string requestData, object jsonParams)
        {
            var request = new RequestInfo(this)
            {
                HttpMethod = "PUT",
                RequestData = requestData,
                JsonData = jsonParams
            };

            return Tuple.Create(await request.TrySend<TResponse>(), request);
        }
    }
}