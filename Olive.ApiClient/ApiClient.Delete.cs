using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public static async Task<bool> Delete(string url, object jsonParams = null, OnApiCallError errorAction = OnApiCallError.Throw)
        {
            var result = await DoDelete<string>(url, jsonParams, errorAction);
            return result.Item2.Error == null;
        }

        public static async Task<TResponse> Delete<TResponse>(
            string url,
            object jsonParams,
            OnApiCallError errorAction = OnApiCallError.Throw)
        {
            return (await DoDelete<TResponse>(url, jsonParams, errorAction)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoDelete<TResponse>(
         string url,
         object jsonParams,
         OnApiCallError errorAction)
        {
            var request = new RequestInfo(url)
            {
                ErrorAction = errorAction,
                HttpMethod = "DELETE",
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send()) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}