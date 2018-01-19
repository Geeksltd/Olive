using System;
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
        public partial class RequestInfo
        {
            const int HTTP_ERROR_STARTING_CODE = 400;
            object jsonData;

            public RequestInfo() { }
            public RequestInfo(string url) => Url = url;

            internal string LocalCachedVersion { get; set; }
            public string Url { get; set; }
            public string HttpMethod { get; set; } = "GET";
            public string ContentType { get; set; }
            public string RequestData { get; set; }
            public string ResponseText { get; set; }
            public OnApiCallError ErrorAction { get; set; } = OnApiCallError.Throw;

            public HttpStatusCode ResponseCode { get; private set; }
            public HttpResponseHeaders ResponseHeaders { get; private set; }
            public Exception Error { get; internal set; }

            public bool EnsureTrailingSlash { get; set; } = true;

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
                        result = ResponseText.ToString().EnsureStartsWith("\"").EnsureEndsWith("\"");
                    return JsonConvert.DeserializeObject<TResponse>(result);
                }
                catch (Exception ex)
                {
                    ex = new Exception("Failed to convert API response to " + typeof(TResponse).GetCSharpName(), ex);
                    LogTheError(ex);

                    ErrorAction.Apply("The server's response was unexpected");
                    return default(TResponse);
                }
            }

            async Task<string> DoSend()
            {
                var url = Url;
                if (EnsureTrailingSlash && url.Lacks("?")) url = url.EnsureEndsWith("/");

                using (var client = new HttpClient())
                {
                    var req = new HttpRequestMessage(new HttpMethod(HttpMethod), url);

                    var sessionToken = GetSessionToken();
                    if (sessionToken.HasValue())
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);

                    if (LocalCachedVersion.HasValue())
                        client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{LocalCachedVersion}\""));

                    if (req.Method != System.Net.Http.HttpMethod.Get)
                    {
                        req.Content = new StringContent(RequestData.OrEmpty(),
                                  System.Text.Encoding.UTF8,
                                    GetContentType());
                    }

                    var errorMessage = "Connection to the server failed.";
                    string responseBody = null;
                    try
                    {
                        var response = await client.SendAsync(req).ConfigureAwait(false);
                        var failed = false;

                        ResponseCode = response.StatusCode;
                        ResponseHeaders = response.Headers;

                        if (LocalCachedVersion.HasValue() && ResponseCode == HttpStatusCode.NotModified)
                            return null;

                        if (((int)ResponseCode) >= HTTP_ERROR_STARTING_CODE)
                        {
                            errorMessage = "Connection to the server failed: " + ResponseCode;
                            failed = true;
                        }

                        responseBody = await response.Content.ReadAsStringAsync();

                        if (failed)
                        {
                            Debug.WriteLine("Server Response: " + responseBody);
                            throw new Exception(errorMessage);
                        }
                        else return responseBody;
                    }
                    catch (Exception ex)
                    {
                        LogTheError(ex);

                        if (System.Diagnostics.Debugger.IsAttached) errorMessage = $"Api call failed: {url}";

                        responseBody = await (ex as WebException)?.GetResponseBody();

                        if (responseBody.OrEmpty().StartsWith("{\"Message\""))
                        {
                            try
                            {
                                var explicitMessage = JsonConvert.DeserializeObject<ServerError>(responseBody).Get(x => x.Message.Or(x.ExceptionMessage));

                                errorMessage = explicitMessage.Or(errorMessage);
                            }
                            catch { /* No logging is needed */; }
                        }
                        // We are doing this in cases that error is not serialized in the SeverError format
                        else errorMessage = responseBody.Or(errorMessage);

                        await ErrorAction.Apply(errorMessage);

                        throw new Exception(errorMessage, ex);
                    }
                }
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