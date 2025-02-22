using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Gpt.ApiDto
{
    class ChatRequestThread
    {
        public ChatRequestThread(ChatMessage[] messages) => Messages = messages;

        [JsonProperty("messages")]
        public ChatMessage[] Messages { get; set; }
    }
}
