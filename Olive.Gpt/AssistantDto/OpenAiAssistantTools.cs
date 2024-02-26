using System.Runtime.Serialization;

namespace Olive.Gpt.AssistantDto;

public enum OpenAiAssistantTools
{
    [EnumMember(Value = "code_interpreter")]
    CodeInterpreter,
    [EnumMember(Value = "retrieval")]
    Retrieval, 
    [EnumMember(Value = "function")]
    Function
}