using Newtonsoft.Json;

namespace Olive.Gpt.ApiDto;

public class Choice
{
    [JsonProperty("delta")]
    public Delta Delta { get; set; }
}