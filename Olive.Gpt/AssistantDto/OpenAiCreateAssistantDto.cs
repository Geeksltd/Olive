using System.Collections.Generic;
using Newtonsoft.Json;

namespace Olive.Gpt.AssistantDto;

public class OpenAiCreateAssistantDto
{
    public string Model { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Instructions { get; set; }
    public OpenAiAssistantTools[] Tools{ get; set; }
    public Dictionary<string,string> Metadata { get; set; }
}