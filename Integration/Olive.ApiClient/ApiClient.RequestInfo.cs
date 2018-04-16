using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            const int HTTP_ERROR_STARTING_CODE = 400;
            object jsonData;
            ApiClient Client;
            HttpRequestMessage RequestMessage;
            HttpClient HttpClient;

            string Url => Client.Url;

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
            public async Task<bool> Send()
            {
                try
                {
                    ResponseText = (await DoSend()).OrEmpty();
                    return true;
                }
                catch (Exception ex)
                {
                    if ((int)ResponseCode >= 400 && (int)ResponseCode < 500)
                        throw ex; // It contains user message.

                    LogTheError(ex);
                    return false;
                }
            }

            public TResponse ExtractResponse<TResponse>()
            {
                // Handle void calls
                if (ResponseText.LacksAll() && typeof(TResponse) == typeof(bool))
                    return default(TResponse);

                try
                {
                    var result = ResponseText;
                    if (typeof(TResponse) == typeof(string) && result.HasValue())
                        result = ResponseText.EnsureStartsWith("\"").EnsureEndsWith("\"");
                    return JsonConvert.DeserializeObject<TResponse>(result);
                }
                catch (Exception ex)
                {
                    ex = new Exception("Failed to convert API response to " + typeof(TResponse).GetCSharpName(), ex);
                    LogTheError(ex);

                    Client.ErrorAction.Apply("The server's response was unexpected");
                    return default(TResponse);
                }
            }

            HttpClient CreateHttpClient()
            {
                var container = new HttpClientHandler { CookieContainer = Client.RequestCookies };
                HttpClient = new HttpClient(container)
                {
                    Timeout = Config.Get("ApiClient:Timeout", 20).Seconds()
                };

                foreach (var config in Client.RequestHeadersCustomizers)
                    config(HttpClient.DefaultRequestHeaders);

                return HttpClient;
            }

            HttpRequestMessage CreateRequestMessage()
            {
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
                using (CreateRequestMessage())
                {
                    var errorMessage = "Connection to the server failed.";
                    string responseBody = null;
                    try
                    {
                        var response = await Client.SendAsync(HttpClient, RequestMessage)
                            .ConfigureAwait(continueOnCapturedContext: false);

                        ResponseCode = response.StatusCode;
                        ResponseHeaders = response.Headers;

                        if (ResponseCode == HttpStatusCode.NotModified)
                            if (LocalCachedVersion.HasValue()) return null;

                        if (((int)ResponseCode) >= HTTP_ERROR_STARTING_CODE)
                        {
                            errorMessage = "Connection to the server failed: " + ResponseCode;
                            responseBody = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine("Server Response: " + responseBody);
                            throw new Exception(errorMessage);
                        }
                        else
                        {
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw await ImproveException(ex, errorMessage, responseBody);
                    }
                }
            }

            async Task<Exception> ImproveException(Exception ex, string errorMessage, string responseBody)
            {
                LogTheError(ex);

                if (Debugger.IsAttached) errorMessage = $"Api call failed: {Url}";

                if (ex is WebException webEx)
                    responseBody = await webEx.GetResponseBody();

                if (responseBody.HasValue())
                {
                    if (responseBody.StartsWith("{\"Message\""))
                    {
                        try
                        {
                            var serverError = JsonConvert.DeserializeObject<ServerError>(responseBody);
                            if (serverError != null)
                                errorMessage = serverError.Message.Or(serverError.ExceptionMessage).Or(errorMessage);
                        }
                        catch { /* No logging is needed */; }
                    }
                    // We are doing this in cases that error is not serialized in the SeverError format
                    else if (responseBody.Contains("<div class=\"titleerror\">"))
                    {
                        errorMessage += Environment.NewLine +
                            responseBody.TrimBefore("<div class=\"titleerror\">", trimPhrase: true)
                             .TrimAfter("</div>").HtmlDecode();
                    }
                    else
                        errorMessage += Environment.NewLine + responseBody;
                }

                await Client.ErrorAction.Apply(errorMessage);

                return new Exception(errorMessage, ex);
            }

            void LogTheError(Exception ex)
            {
                Error = ex;
                Debug.WriteLine($"Http{HttpMethod} failed -> {Url}");
                Debug.WriteLine(ex);
            }
        }
    }
}