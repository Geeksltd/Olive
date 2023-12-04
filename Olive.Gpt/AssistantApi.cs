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
        readonly JsonSerializerSettings Settings = new() { NullValueHandling = NullValueHandling.Ignore };
        readonly HttpClient Client = new(CreateForgivingHandler()) { Timeout = 60.Seconds() };
        string Model;

        public AssistantApi(string apiKey, string model = "gpt-3.5-turbo")
        {
            Model = model;
            Client.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
            Client.DefaultRequestHeaders.Add("User-Agent", "olive/dotnet_openai_api");
            Client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
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
            var response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi New Thread API to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiThreadDto>(result);

            return jObject.Id;
        }

        public async Task<string> AddMessageToThread(ChatMessage[] messages, string threadId)
        {
            var jsonContent = JsonConvert.SerializeObject(new ChatRequest(messages) { Model = Model }, Settings);
            var payload = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url) { Content = payload };
            var response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi Add Message API to get completion. HTTP status code: " + response.StatusCode +
                    ". Request body: " + jsonContent +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiMessageDto>(result);

            return jObject.Id;
        }

        public async Task<string> CreateNewRun(string assistantId, string threadId)
        {
            var payload = new StringContent(JsonConvert.SerializeObject(new { assistant_id = assistantId }, Settings), Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.openai.com/v1/threads/{threadId}/runs") { Content = payload };
            var response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi New Run API to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiRunDto>(result);

            return jObject.Id;
        }

        public async Task<OpenAiRunStatus> GetRunStatus(string threadId, string runId)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
            var response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi Get Run API to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiRunDto>(result);

            return jObject.Status;
        }
        
        public async Task<OpenAiMessageDto[]> GetThreadMessages(string threadId, string lastMessageId)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.openai.com/v1/threads/{threadId}/messages?limit=100&after={lastMessageId}&order=asc");
            var response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi Get Run API to get completion. HTTP status code: " + response.StatusCode +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<OpenAiMessagesDto>(result);

            return jObject.Data;
        }
    }
}