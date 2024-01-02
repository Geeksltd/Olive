using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive.Gpt
{
    public class Api
    {
        readonly JsonSerializerSettings Settings = new() { NullValueHandling = NullValueHandling.Ignore };
        readonly HttpClient Client = new(CreateForgivingHandler()) { Timeout = 60.Seconds() };
        string Model;

        public Api(string apiKey, string model = "gpt-3.5-turbo")
        {
            Model = model;
            Client.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
            Client.DefaultRequestHeaders.Add("User-Agent", "olive/dotnet_openai_api");
        }

        static HttpClientHandler CreateForgivingHandler() => new()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        public Task<string> GetResponse(Command command) => GetResponse(command.ToString());

        public Task<string> GetResponse(string command) => GetResponse(new[] { new ChatMessage("user", command) });
      
        public async Task<string> GetResponse(string command, IEnumerable<string> steps)
        {
            var enumerable = steps as string[] ?? steps.ToArray();
            if (!enumerable.Any()) return await GetResponse(command);

            var result = await GetResponse(new[] { new ChatMessage("user", command) });
            foreach (var step in enumerable)
            {
                var stepCommand = step.Replace("#CURRENT_RESULT#", result);
                result = await GetResponse(new[] { new ChatMessage("user", stepCommand) });
            }

            return result;
        }

        public async Task<string> GetResponse(ChatMessage[] messages)
        {
            var jsonContent = JsonConvert.SerializeObject(new ChatRequest(messages) { Model = Model }, Settings);
            var payload = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions") { Content = payload };
            var response = await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Error calling OpenAi API to get completion. HTTP status code: " + response.StatusCode +
                    ". Request body: " + jsonContent +
                    ". Response body: " + await response.Content.ReadAsStringAsync());

            var result = new StringBuilder();

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                while (await reader.ReadLineAsync() is { } line)
                {
                    if (line.StartsWith("data: ")) line = line.Substring("data: ".Length);
                    if (line == "[DONE]") break;

                    if (line.HasValue())
                    {
                        var token = JsonConvert.DeserializeObject<ChatResponse>(line).ToString();
                        if (token.HasValue()) result.Append(token);
                    }
                }
            }

            return result.ToString();
        }
    }
}