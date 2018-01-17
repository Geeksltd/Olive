
//    using System;
//    using System.Threading.Tasks;

//    /// <summary>
//    /// Wraps a GET API call response. Provides caching and graceful exception handling features.
//    /// </summary>
//    public class ApiResponse<TData>
//    {
//        internal TData ResultData;

//        internal static ApiResponse<TData> FromFresh(string url, TData freshResult)
//        {
//            return new ApiResponse<TData> { RequestUrl = url, ResultData = freshResult };
//        }

//        internal static ApiResponse<TData> FromCache(string url, TData cachedData)
//        {
//            return new ApiResponse<TData> { RequestUrl = url, ResultData = cachedData, IsFromCache = true };
//        }

//        internal static async Task<ApiResponse<TData>> FromError(string url, Exception error, TData cachedData, bool isCacheAvailable)
//        {
//            if (!await Device.Network.IsAvailable())
//                error = new Exception("Internet connection is unavailable.", error);
//            else
//            {
//                Device.Log.Error(error.ToFullMessage());
//                error = new Exception("An error occured when connecting to the server.", error);
//            }

//            return new ApiResponse<TData>
//            {
//                RequestUrl = url,
//                Error = error,
//                ResultData = cachedData,
//                IsFromCache = isCacheAvailable
//            };
//        }

//        public string RequestUrl { get; internal set; }

//        /// <summary>
//        /// The exception that occured when executing the API call.
//        /// </summary>
//        public Exception Error { get; internal set; }

//        public bool IsFromCache { get; internal set; }

//        /// <summary>
//        /// Gets the result obtained from a HttpGet request.        
//        /// </summary>
//        public async Task<TData> Result(OnError errorAction = OnError.Toast, ApiResponseCache cacheChoice = ApiResponseCache.Accept)
//        {
//            // Fresh result - All happy:
//            if (Error == null) return ResultData;

//            // Error occured, but cache is available:
//            if (IsFromCache && cacheChoice != ApiResponseCache.Refuse) return ResultData;

//            await errorAction.Apply(Error);
//            return default(TData);
//        }
//    }
// }