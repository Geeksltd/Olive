using Newtonsoft.Json;

namespace Olive.Gpt.ApiDto
{
    class ChatRequest
    {
        public ChatRequest(ChatMessage[] messages) => Messages = messages;

        [JsonProperty("model")]
        public string Model { get; set; } = "gpt-3.5-turbo";

        [JsonProperty("messages")]

        public ChatMessage[] Messages { get; set; }

        [JsonProperty("stream")] public bool Stream { get; set; } = true;
        [JsonProperty("response_format")] public ResponseFormat ResponseFormat { get; set; }
    }
}
