using Newtonsoft.Json;

namespace Olive.Gpt
{
    public class ChatMessage
    {
        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
