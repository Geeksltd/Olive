using System.Runtime.Serialization;

namespace Olive.Gpt.AssistantDto.V2;

public enum OpenAiAssistantTools
{
    [EnumMember(Value = "code_interpreter")]
    CodeInterpreter,
    [EnumMember(Value = "file_search")]
    FileSearch, 
    [EnumMember(Value = "function")]
    Function
}