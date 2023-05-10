using System.Linq;
using Newtonsoft.Json;

namespace Olive.Gpt
{
    class ChatResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public Choice[] Choices { get; set; }

        public override string ToString() => Choices.FirstOrDefault()?.Delta?.Content;
    }

    public class Choice
    {
        [JsonProperty("delta")]
        public Delta Delta { get; set; }
    }

    public class Delta
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}