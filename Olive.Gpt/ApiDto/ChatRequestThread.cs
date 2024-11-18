using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Gpt.ApiDto
{
    class ChatRequestThread
    {
        public ChatRequestThread(ChatMessage[] messages)
        {
            foreach (var item in messages)
            {
                Messages.Add(item.Role, item.Content);
            }
        }

        [JsonProperty("messages")]
        public Dictionary<string, object> Messages { get; set; }
    }
}
