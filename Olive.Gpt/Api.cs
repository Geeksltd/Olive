using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Olive.Gpt.ApiDto;
using Olive.Gpt.DalleDto;

namespace Olive.Gpt
{
    public class Api
    {
        public const string CurrentResultPlaceholder = "#CURRENT_RESULT#";
        static readonly JsonSerializerSettings Settings = new() { NullValueHandling = NullValueHandling.Ignore, };
        static readonly HttpClient Client = new(CreateForgivingHandler()) { Timeout = 5.Minutes() };
        readonly string _model;

        public Api(string apiKey, string model = "gpt-3.5-turbo")
        {
            if (apiKey.IsEmpty()) throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            Client.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
            Client.DefaultRequestHeaders.Add("User-Agent", "olive/dotnet_openai_api");
        }

        static HttpClientHandler CreateForgivingHandler() => new()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        public Task<string> GetResponse(Command command, string model = null, ResponseFormats responseFormat = ResponseFormats.NotSet) => GetResponse(command.ToString(), model, responseFormat);

        public Task<string> GetResponse(string command, string model = null, ResponseFormats responseFormat = ResponseFormats.NotSet) => GetResponse(new[] { new ChatMessage("user", command) }, model, responseFormat);

        public async Task<string> GetTransformationResponse(IEnumerable<string> steps, ResponseFormats responseFormat = ResponseFormats.JsonObject)
        {
            var enumerable = steps as string[] ?? steps.ToArray();
            if (!enumerable.Any())
                throw new Exception("Transformation steps is empty");

            var result = "";
            foreach (var step in enumerable)
            {
                var stepCommand = step.Replace(CurrentResultPlaceholder, result);
                result = await GetResponse(new[] { new ChatMessage("user", stepCommand) }, null, responseFormat);
            }

            return result;
        }

        public async Task<string> GetTransformationResponse(IEnumerable<(string prompt, string model)> steps, ResponseFormats responseFormat = ResponseFormats.JsonObject)
        {
            var enumerable = steps as (string prompt, string model)[] ?? steps.ToArray();
            if (!enumerable.Any())
                throw new Exception("Transformation steps is empty");

            var result = "";
            foreach (var (prompt, model) in enumerable)
            {
                var stepCommand = prompt.Replace(CurrentResultPlaceholder, result);
                result = await GetResponse(new[] { new ChatMessage("user", stepCommand) }, model, responseFormat);
            }

            return result;
        }

        public async Task<string> GetResponse(ChatMessage[] messages, string model = null, ResponseFormats responseFormat = ResponseFormats.NotSet)
        {
            if (!messages.Any() || messages.All(m => m.Content.IsEmpty()))
            {
                throw new ArgumentNullException(nameof(messages));
            }

            if (model.Or(_model).StartsWith("dall-e-")) return await GenerateDalleImage(messages[0].Content, model);

            var request = new ChatRequest(messages) { Model = model.Or(_model) };
            if (responseFormat == ResponseFormats.JsonObject && messages.Any(a=>a.Content.Contains("json",false)))
            {
                request.ResponseFormat = new ResponseFormat { Type = responseFormat };
            }
            var jsonContent = JsonConvert.SerializeObject(request, Settings);
            var payload = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions") { Content = payload };

            HttpResponseMessage response;

            try
            {
                response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception e)
            {
                Log.For<Api>().Error(e, "Gpt Query FAILED, Request body: " + jsonContent);
                return null;
            }

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Error calling OpenAi API to get completion. HTTP status code: " + response.StatusCode + ". Request body: " + jsonContent + ". Response body: " + await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();

            //using (var stream = await response.Content.ReadAsStreamAsync())
            //using (var reader = new StreamReader(stream))
            //{
            //    while (await reader.ReadLineAsync() is { } line)
            //    {
            //        if (line.StartsWith("data: ")) line = line.Substring("data: ".Length);
            //        if (line == "[DONE]") break;

            //        if (line.HasValue())
            //        {
            //            var token = JsonConvert.DeserializeObject<ChatResponse>(line)?.ToString();
            //            if (token.HasValue()) result.Append(token);
            //        }
            //    }
            //}

            //return result.Length > 0 ? result.ToString() : null;
        }

        public async Task<string> GenerateDalleImage(string prompt, string model = "dall-e-3", Dictionary<string, object> parameters = null)
        {
            const string api = "https://api.openai.com/v1/images/generations";

            var requestPayload = parameters ?? new Dictionary<string, object>();
            if (!requestPayload.ContainsKey("prompt")) requestPayload.Add("prompt", prompt);
            if (!requestPayload.ContainsKey("model")) requestPayload.Add("model", model);
            if (!requestPayload.ContainsKey("size")) requestPayload.Add("size", "1024x1024");
            if (!requestPayload.ContainsKey("quality")) requestPayload.Add("quality", "standard");
            if (!requestPayload.ContainsKey("n")) requestPayload.Add("n", 1);

            var jsonContent = JsonConvert.SerializeObject(requestPayload, Settings);
            var payload = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, api) { Content = payload };

            HttpResponseMessage response;

            try
            {
                response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception e)
            {
                Log.For<Api>().Error(e, "Dall-E Image Query FAILED, Request body: " + jsonContent);
                return null;
            }

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Error calling OpenAi API for Dall-E image generation. HTTP status code: " + response.StatusCode + ". Request body: " + jsonContent+ ". Response body: " + await response.Content.ReadAsStringAsync());

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<DalleResponse>(responseContent);

            return responseObject?.Data?[0]?.Url;
        }
    }
}