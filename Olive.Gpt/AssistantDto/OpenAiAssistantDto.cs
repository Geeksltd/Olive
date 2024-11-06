namespace Olive.Gpt.AssistantDto;

public class OpenAiAssistantDto: OpenAiCreateAssistantDto, IOpenAiAssistantDto
{
    public string Id { get; set; }
}