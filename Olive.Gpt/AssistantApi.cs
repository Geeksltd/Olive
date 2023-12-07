using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Gpt.AssistantDto;

namespace Olive.Gpt
{
    public class AssistantApi
    {
        readonly JsonSerializerSettings _settings = new() { NullValueHandling = NullValueHandling.Ignore };
        readonly HttpClient _client = new(CreateForgivingHandler()) { Timeout = 60.Seconds() };

        public AssistantApi(string apiKey)
        {
            _client.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
            _client.DefaultRequestHeaders.Add("User-Agent", "olive/dotnet_openai_api");
            _client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
        }

        static HttpClientHandler CreateForgivingHandler() => new()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        public async Task<string> CreateNewThread()
        {
            var payload = new StringContent("", Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/threads") { Content = payload };
            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi New Thread API to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiThreadDto>(result);

            return jObject.Id;
        }

        public async Task<string> AddMessageToThread(ChatMessage message, string threadId)
        {
            if (threadId.IsEmpty()) throw new Exception("Thread Id is empty");
            if (message.Content.IsEmpty()) throw new Exception("Message is empty");
           
            if (message.Role.IsEmpty())
            {
                message.Role = "user";
            }

            var jsonContent = JsonConvert.SerializeObject(message, _settings);
            var payload = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url) { Content = payload };
            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Error calling OpenAi Add Message to {threadId} API to get completion. " +
                    $"HTTP status code: {response.StatusCode}. " +
                    $"Request body: {jsonContent}. " +
                    $"Response body: {await response.Content.ReadAsStringAsync()}");

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiMessageDto>(result);

            return jObject.Id;
        }

        public async Task<string> CreateNewRun(string assistantId, string threadId)
        {
            if (assistantId.IsEmpty()) throw new Exception("Assistant Id is empty");
            if (threadId.IsEmpty()) throw new Exception("Thread Id is empty");
           
            var payload = new StringContent(JsonConvert.SerializeObject(new { assistant_id = assistantId }, _settings), Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.openai.com/v1/threads/{threadId}/runs") { Content = payload };
            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Error calling OpenAi New Run API ({assistantId} - {threadId}) to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiRunDto>(result);

            return jObject.Id;
        }

        public async Task<OpenAiRunStatus> GetRunStatus(string threadId, string runId)
        {
            if (runId.IsEmpty()) throw new Exception("Run Id is empty");
            if (threadId.IsEmpty()) throw new Exception("Thread Id is empty");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Error calling OpenAi Get Run Status API ({threadId} - {runId}) to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiRunDto>(result);

            return jObject.Status;
        }
        
        public async Task<OpenAiMessageDto[]> GetThreadMessages(string threadId, string lastMessageId)
        {
            if (lastMessageId.IsEmpty()) throw new Exception("Last Message Id is empty");
            if (threadId.IsEmpty()) throw new Exception("Thread Id is empty");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.openai.com/v1/threads/{threadId}/messages?limit=100&after={lastMessageId}&order=asc");
            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Error calling OpenAi Get Messages API ({threadId} - {lastMessageId}) to get completion. " +
                    $"HTTP status code: {response.StatusCode}. " +
                    $"Response body: {await response.Content.ReadAsStringAsync()}");

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiMessagesDto>(result);

            return jObject.Data;
        }
    }
}