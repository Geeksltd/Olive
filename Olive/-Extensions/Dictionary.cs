using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Olive
{
    static partial class OliveExtensions
    {
        public static JObject ToJson(this IEnumerable<KeyValuePair<string, string>> @this)
        {
            var result = new JObject();

            foreach (var item in @this)
                result.Add(item.Key, item.Value);

            return result;
        }
    }
}