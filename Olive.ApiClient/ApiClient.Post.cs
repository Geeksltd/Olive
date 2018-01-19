using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public static async Task<bool> Post(
          string url,
          OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoPost<string>(url, null, null, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<bool> Post(
            string url,
            string requestData,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoPost<string>(url, requestData, null, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<bool> Post(
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoPost<string>(url, null, jsonParams, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<TResponse> Post<TResponse>(
            string url,
            string requestData,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            return (await DoPost<TResponse>(url, requestData, null, errorAction)).Item1;
        }

        public static async Task<TResponse> Post<TResponse>(
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            return (await DoPost<TResponse>(url, null, jsonParams, errorAction)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoPost<TResponse>(
         string url,
         string requestData,
         object jsonParams,
         OnApiCallError errorAction)
        {
            var request = new RequestInfo(url)
            {
                ErrorAction = errorAction,
                HttpMethod = "POST",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send()) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}