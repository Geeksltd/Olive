using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public static async Task<bool> Put(
          string url,
          OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoPut<string>(url, null, null, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<bool> Put(
            string url,
            string requestData,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoPut<string>(url, requestData, null, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<bool> Put(
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoPut<string>(url, null, jsonParams, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<TResponse> Put<TResponse>(
            string url,
            string requestData,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            return (await DoPut<TResponse>(url, requestData, null, errorAction)).Item1;
        }

        public static async Task<TResponse> Put<TResponse>(
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            return (await DoPut<TResponse>(url, null, jsonParams, errorAction)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoPut<TResponse>(
         string url,
         string requestData,
         object jsonParams,
         OnApiCallError errorAction)
        {
            var request = new RequestInfo(url)
            {
                ErrorAction = errorAction,
                HttpMethod = "PUT",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send()) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}