using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public async Task<bool> Post()
        {
            var result = await DoPost<string>(null, null);
            return result.Item2.Error == null;
        }

        public async Task<bool> Post(string requestData)
        {
            var result = await DoPost<string>(requestData, null);
            return result.Item2.Error == null;
        }

        public async Task<bool> Post(object jsonParams)
        {
            var result = await DoPost<string>(null, jsonParams);
            return result.Item2.Error == null;
        }

        public async Task<TResponse> Post<TResponse>(string requestData)
        {
            return (await DoPost<TResponse>(requestData, null)).Item1;
        }

        public async Task<TResponse> Post<TResponse>(object jsonParams)
        {
            return (await DoPost<TResponse>(null, jsonParams)).Item1;
        }

        async Task<Tuple<TResponse, RequestInfo>> DoPost<TResponse>(string requestData, object jsonParams)
        {
            var request = new RequestInfo(this)
            {
                HttpMethod = "POST",
                RequestData = requestData,
                JsonData = jsonParams
            };

            return Tuple.Create(await request.TrySend<TResponse>(), request);
        }
    }
}