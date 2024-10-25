using System.Collections.Generic;
using Newtonsoft.Json;

namespace Olive.Gpt.AssistantDto.V2;

public class OpenAiCreateAssistantDto : Olive.Gpt.AssistantDto.OpenAiCreateAssistantDto
{
    public new OpenAiAssistantToolsDto[] Tools{ get; set; }
}

public class OpenAiAssistantToolsDto : Olive.Gpt.AssistantDto.OpenAiAssistantToolsDto
{
    [JsonProperty("type")]
    public new OpenAiAssistantTools Type{ get; set; }
}
