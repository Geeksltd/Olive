using System.Collections.Generic;
using Newtonsoft.Json;

namespace Olive.Gpt.AssistantDto.V2;
/// <summary>
/// model for create V2 assistant
/// </summary>
public class OpenAiCreateAssistantDto : Olive.Gpt.AssistantDto.OpenAiCreateAssistantDto
{
    [JsonProperty("tools")]
    public new OpenAiAssistantToolsDto[] Tools{ get; set; }
}
/// <summary>
/// tools of V2 assistant
/// </summary>
public class OpenAiAssistantToolsDto : Olive.Gpt.AssistantDto.OpenAiAssistantToolsDto
{
    [JsonProperty("type")]
    public new OpenAiAssistantTools Type{ get; set; }
}
