using System.Runtime.Serialization;

namespace Olive.Gpt.AssistantDto.V2;
/// <summary>
/// V2 assistant tools
/// </summary>
public enum OpenAiAssistantTools
{
    /// <summary>
    /// replace with the code_interpreter on serialization
    /// </summary>
    [EnumMember(Value = "code_interpreter")]
    CodeInterpreter,
    /// <summary>
    /// replace with the file_search on serialization
    /// </summary>
    [EnumMember(Value = "file_search")]
    FileSearch,
    /// <summary>
    /// replace with the function on serialization
    /// </summary>
    [EnumMember(Value = "function")]
    Function
}