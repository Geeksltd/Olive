using System;
using System.Threading.Tasks;

namespace Olive.ApiClient
{
    partial class ApiHttpClient
    {
        public static async Task<bool> Put(
          string relativeUrl,
          OnError errorAction = OnError.Throw,
          bool showWaiting = true)
        {
            var result = await DoPut<string>(relativeUrl, null, null, errorAction, showWaiting);
            return result.Item2.Error == null;
        }

        public static async Task<bool> Put(
            string relativeUrl,
            string requestData,
            OnError errorAction = OnError.Throw,
            bool showWaiting = true)
        {
            var result = await DoPut<string>(relativeUrl, requestData, null, errorAction, showWaiting);
            return result.Item2.Error == null;
        }

        public static async Task<bool> Put(
            string relativeUrl,
            object jsonParams,
            OnError errorAction = OnError.Throw,
            bool showWaiting = true)
        {
            var result = await DoPut<string>(relativeUrl, null, jsonParams, errorAction, showWaiting);
            return result.Item2.Error == null;
        }

        public static async Task<TResponse> Put<TResponse>(
            string relativeUrl,
            string requestData,
            OnError errorAction = OnError.Throw,
            bool showWaiting = true)
        {
            return (await DoPut<TResponse>(relativeUrl, requestData, null, errorAction, showWaiting)).Item1;
        }

        public static Task<TResponse> Put<TResponse, TEntity>(
           TEntity entity,
           string relativeUrl,
           string requestData,
           OnError errorAction = OnError.Throw,
           bool showWaiting = true) where TEntity : IQueueable<Guid>
        {
            return Put<TResponse, TEntity, Guid>(entity, relativeUrl, requestData, errorAction, showWaiting);
        }

        public static async Task<TResponse> Put<TResponse, TEntity, TIdentifier>(
           TEntity entity,
           string relativeUrl,
           string requestData,
           OnError errorAction = OnError.Throw,
           bool showWaiting = true) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPut<TResponse, TEntity, TIdentifier>(entity, relativeUrl, requestData, null, errorAction, showWaiting)).Item1;
        }

        public static async Task<TResponse> Put<TResponse>(
            string relativeUrl,
            object jsonParams,
            OnError errorAction = OnError.Throw,
            bool showWaiting = true)
        {
            return (await DoPut<TResponse>(relativeUrl, null, jsonParams, errorAction, showWaiting)).Item1;
        }

        public static Task<TResponse> Put<TResponse, TEntity>(
            TEntity entity,
            string relativeUrl,
            object jsonParams,
            OnError errorAction = OnError.Throw,
            bool showWaiting = true) where TEntity : IQueueable<Guid>
        {
            return Put<TResponse, TEntity, Guid>(entity, relativeUrl, jsonParams, errorAction, showWaiting);
        }

        public static async Task<TResponse> Put<TResponse, TEntity, TIdentifier>(
            TEntity entity,
            string relativeUrl,
            object jsonParams,
            OnError errorAction = OnError.Throw,
            bool showWaiting = true) where TEntity : IQueueable<TIdentifier>
        {
            return (await DoPut<TResponse, TEntity, TIdentifier>(entity, relativeUrl, null, jsonParams, errorAction, showWaiting)).Item1;
        }

        static async Task<Tuple<TResponse, RequestInfo>> DoPut<TResponse>(
         string relativeUrl,
         string requestData,
         object jsonParams,
         OnError errorAction,
         bool showWaiting)
        {
            var request = new RequestInfo(relativeUrl)
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

        static async Task<Tuple<TResponse, RequestInfo>> DoPut<TResponse, TEntity, TIdentifier>(
         TEntity entity,
         string relativeUrl,
         string requestData,
         object jsonParams,
         OnError errorAction,
         bool showWaiting) where TEntity : IQueueable<TIdentifier>
        {
            var request = new RequestInfo(relativeUrl)
            {
                ErrorAction = errorAction,
                HttpMethod = "PUT",
                RequestData = requestData,
                JsonData = jsonParams
            };

            var result = default(TResponse);
            if (await request.Send<TEntity, TIdentifier>(entity)) result = request.ExtractResponse<TResponse>();
            return Tuple.Create(result, request);
        }
    }
}