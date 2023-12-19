using System.Runtime.Serialization;

namespace Olive.Gpt.AssistantDto;

public enum OpenAiAssistantTools
{
    [EnumMember(Value = "code_interpreter")]
    CodeInterpreter,
    Retrieval, 
    Function
}