using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        public bool EnsureTrailingSlash { get; set; } = true;

        List<Action<HttpRequestHeaders>> RequestHeadersCustomizers = new List<Action<HttpRequestHeaders>>();

        public ApiClient Header(Action<HttpRequestHeaders> config)
        {
            if (config != null) RequestHeadersCustomizers.Add(config);
            return this;
        }

        public partial class RequestInfo
        {
            object jsonData;
            ApiClient Client;
            HttpRequestMessage RequestMessage;
            HttpClient HttpClient;

            string Url => Client.Url;

            /// <summary>The url without its query string, which can carry tokens, for logging. Debug
            /// included: some hosts keep it in production.</summary>
            string UrlForLog => Url.OrEmpty().Split('?')[0];

            int Attempts = 1;

            public RequestInfo(ApiClient client) => Client = client;

            internal string LocalCachedVersion { get; set; }
            public string HttpMethod { get; set; } = "GET";
            public string ContentType { get; set; }
            public string RequestData { get; set; }
            public string ResponseText { get; set; }

            public HttpStatusCode ResponseCode { get; private set; }
            public HttpResponseHeaders ResponseHeaders { get; private set; }
            public Exception Error { get; internal set; }

            public string GetContentType()
            {
                return ContentType.Or("application/x-www-form-urlencoded".Unless(HttpMethod == "GET"));
            }

            public object JsonData
            {
                get => jsonData;
                set
                {
                    jsonData = value;

                    if (value != null)
                    {
                        ContentType = "application/json";
                        RequestData = JsonConvert.SerializeObject(value);
                    }
                }
            }

            /// <summary>
            /// Sends this request to the server and processes the response.
            /// The error action will also apply.
            /// It will return whether the response was successfully received.
            /// </summary>
            public async Task<TResponse> TrySend<TResponse>()
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    ResponseText = (await DoSend()).OrEmpty();
                    return ExtractResponse<TResponse>();
                }
                catch (Exception ex)
                {
                    Error = ex;

                    var rethrow = ResponseCode.ContainsUserMessage() || Client.FallBackEventPolicy == ApiFallBackEventPolicy.Raise;
                    LogFailure(ex, watch.Elapsed, rethrow);

                    if (rethrow) throw;
                    return default(TResponse);
                }
            }

            public TResponse ExtractResponse<TResponse>()
            {
                // Handle void calls
                if (ResponseText.LacksAll() && typeof(TResponse) == typeof(bool))
                {
                    Log.For(this).Debug("ExtractResponse: ResponseText is empty for " + Url);
                    return default(TResponse);
                }

                try
                {
                    var response = ResponseText;

                    if (typeof(TResponse) == typeof(string) && response.HasValue())
                        response = ResponseText.EnsureStartsWith("\"").EnsureEndsWith("\"");

                    TResponse result;

                    if (typeof(TResponse) == typeof(string))
                    {
                        result = (TResponse)(object)response;
                    }
                    else
                    {
                        result = JsonConvert.DeserializeObject<TResponse>(response);
                        Log.For(this).Trace("ExtractResponse: Deserialized Result: " + result);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to convert API response to " + typeof(TResponse).GetCSharpName(), ex);
                }
            }

            HttpClient CreateHttpClient()
            {
                var container = new HttpClientHandler { CookieContainer = Client.RequestCookies };

                container.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient = Client.Factory.CreateClient();
                HttpClient.Timeout = Context.Current.Config.GetValue("ApiClient:Timeout", 60).Seconds();

                foreach (var config in Client.RequestHeadersCustomizers)
                    config(HttpClient.DefaultRequestHeaders);

                return HttpClient;
            }

            /// <summary>Creates the message for one attempt, replacing the one before it, which HttpClient
            /// will not send again.</summary>
            HttpRequestMessage CreateRequestMessage()
            {
                RequestMessage?.Dispose();
                RequestMessage = new HttpRequestMessage(new HttpMethod(HttpMethod), Url);

                if (RequestMessage.Method != System.Net.Http.HttpMethod.Get)
                    RequestMessage.Content = new StringContent(RequestData.OrEmpty(),
                              System.Text.Encoding.UTF8, GetContentType());

                return RequestMessage;
            }

            async Task<string> DoSend()
            {
                if (Client.EnsureTrailingSlash && Url.Lacks("?")) Client.Url = Url;

                using (CreateHttpClient())
                {
                    string responseBody = null;
                    var watch = Stopwatch.StartNew();

                    try
                    {
                        var response = await Client.SendAsync(HttpClient, CreateRequestMessage, OnRetry)
                            .ConfigureAwait(continueOnCapturedContext: false);

                        ResponseCode = response.StatusCode;
                        ResponseHeaders = response.Headers;

                        Log.For(this).Debug($"{HttpMethod} {UrlForLog} returned {(int)ResponseCode} {ResponseCode} in {watch.ElapsedMilliseconds}ms");

                        if (ResponseCode == HttpStatusCode.NotModified && LocalCachedVersion.HasValue())
                            return null;

                        responseBody = await response.Content.ReadAsStringAsync();

                        if (ResponseCode.IsError())
                        {
                            throw new Exception("Server error " + ResponseCode);
                        }
                        else
                        {
                            Log.For(this).Trace("DoSend result: " + responseBody);
                            return responseBody;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException?.Message == "The HTTP redirect request failed")
                            throw new Exception("The Api target seems misconfigured as it's trying to redirect! Consider using [AuthorizeApi] instead of [Authorize].", ex);

                        throw await ImproveException(ex, responseBody);
                    }
                    finally
                    {
                        RequestMessage?.Dispose();
                    }
                }
            }

            void OnRetry(Exception ex, int attempt)
            {
                Attempts = attempt + 1;
                Log.For(this).Warning($"{HttpMethod} {UrlForLog} failed: {ex.Message} Retrying {attempt}/{Client.retries}.");
            }

            static string ExtractUserFriendlyErrorMessage(string responseBody, bool isUserMessage)
            {
                if (responseBody.StartsWith("{\"Message\""))
                {
                    try
                    {
                        var serverError = JsonConvert.DeserializeObject<ServerError>(responseBody);

                        if (serverError != null)
                            return serverError.Message.Or(serverError.ExceptionMessage);
                    }
                    catch { /* No logging is needed */; }
                }

                if (responseBody.Contains("<div class=\"titleerror\">"))
                {
                    return responseBody.RemoveBeforeAndIncluding("<div class=\"titleerror\">").RemoveFrom("</div>").HtmlDecode();
                }

                // A 4xx body is the message for the caller, who gets it whole (and may parse it).
                if (isUserMessage) return responseBody;

                // Not a message meant for anyone: an error page, say. Only its start, as this becomes the
                // exception's message, which is logged wherever it is caught - and a developer error page
                // carries the request's headers and cookies.
                return responseBody.Summarize(200, enforceMaxLength: true);
            }

            async Task<Exception> ImproveException(Exception ex, string responseBody)
            {
                // Without the query string: this message is logged, and shown to whoever catches it.
                var errorMessage = $"Api call failed: {UrlForLog}";

                if (ex is WebException webEx)
                    responseBody = await webEx.GetResponseBody();

                if (responseBody.HasValue())
                    errorMessage = ExtractUserFriendlyErrorMessage(responseBody, ResponseCode.ContainsUserMessage());

                return new Exception(errorMessage.Or(ex.Message), ex);
            }

            void LogFailure(Exception ex, TimeSpan elapsed, bool rethrown)
            {
                var status = ResponseCode == 0 ? "" : $" with {(int)ResponseCode} {ResponseCode}";
                var message = $"{HttpMethod} {UrlForLog} failed{status} after {Attempts} attempt(s) in {elapsed.TotalMilliseconds:0}ms.";


                // A warning when someone else still decides the outcome: a rethrown failure reaches the caller,
                // and a GET may yet be answered from the cache. A write whose failure is swallowed here has no
                // one else to report it.
                if (rethrown || HttpMethod == "GET") Log.For(this).Warning(ex, message);
                else Log.For(this).Error(ex, message + " Returning the default result.");
            }
        }
    }
}