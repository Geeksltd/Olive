using System.Runtime.Serialization;

namespace Olive.Gpt.AssistantDto;

public enum OpenAiRunStatus
{
    Queued,
    [EnumMember(Value = "in_progress")]
    InProgress,
    [EnumMember(Value = "requires_action")]
    RequiresAction,
    Cancelling,
    Cancelled,
    Failed,
    Completed,
    Expired
}