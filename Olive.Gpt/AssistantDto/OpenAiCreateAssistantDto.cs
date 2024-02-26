using System.Collections.Generic;
using Newtonsoft.Json;

namespace Olive.Gpt.AssistantDto;

public class OpenAiCreateAssistantDto
{
    [JsonProperty("model")]
    public string Model { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("instructions")]
    public string Instructions { get; set; }
    [JsonProperty("tools")]
    public OpenAiAssistantToolsDto[] Tools{ get; set; }
    [JsonProperty("metadata")]
    public Dictionary<string,string> Metadata { get; set; }
}

public class OpenAiAssistantToolsDto
{
    [JsonProperty("type")]
    public OpenAiAssistantTools Type{ get; set; }
}
