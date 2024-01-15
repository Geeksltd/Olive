using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Olive.Gpt.ApiDto;

public enum ResponseFormats
{
    NotSet,
    [EnumMember(Value = "json_object")][JsonProperty("json_object")] JsonObject
}