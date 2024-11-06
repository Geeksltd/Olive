using Newtonsoft.Json;

namespace Olive.Gpt.AssistantDto.V2;

public class OpenAiAssistantDto: Olive.Gpt.AssistantDto.OpenAiAssistantDto , IOpenAiAssistantDto
{
    public string Id { get; set; }

    [JsonProperty("tools")]
    public V2.OpenAiAssistantToolsDto[] Tools { get; set; }
}