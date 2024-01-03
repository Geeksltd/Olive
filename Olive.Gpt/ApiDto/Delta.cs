using Newtonsoft.Json;

namespace Olive.Gpt.ApiDto;

public class Delta
{
    [JsonProperty("content")]
    public string Content { get; set; }
}