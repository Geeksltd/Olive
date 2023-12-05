namespace Olive.Gpt.AssistantDto;

public class OpenAiMessageDto
{
    public string Id { get; set; }
    public string Role { get; set; }
    public OpenAiMessageContentDto[] Content { get; set; }
}