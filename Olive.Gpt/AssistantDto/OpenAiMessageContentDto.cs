namespace Olive.Gpt.AssistantDto;

public class OpenAiMessageContentDto
{
    public string Type { get; set; }
    public OpenAiMessageContentTextDto Text { get; set; }
}