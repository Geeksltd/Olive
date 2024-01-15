using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Olive.Gpt.ApiDto;

public class ResponseFormat
{
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty("type")] public ResponseFormats Type { get; set; }
}