using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive.OpenAI
{
    public class OpenAI
    {
        ChatClient _client = null;
        /// <summary>
        /// Creates a new instance of OpenAI. If no key is passed it reads the OpenAI:Key from config.
        /// </summary>
        public OpenAI(string? key = null)
        {
            // Get the keys from Config
            var model = Olive.Config.Get("OpenAI:Models:ChatModel").Or("gpt-4o");
            var apiKey = key.Or(Olive.Config.Get("OpenAI:Key"));

            if (apiKey.IsEmpty())
            {
                throw new ArgumentException("The api key was not provided. Please set it in the AppSettings or webConfig in the OpenAI:APIKey entry");
            }
            _client = new(model, apiKey);
        }

        public async Task<string> GetResponse(string[] messages, string instruction = null)
        {
            var chatMessages = GetChatMessages(messages, instruction);
            ChatCompletion completion = await this._client.CompleteChatAsync(chatMessages);
            return completion.Content[0].Text;
        }

        public async IAsyncEnumerable<string> GetResponseStream(string[] messages, string instruction = null)
        {
            var chatMessages = GetChatMessages(messages, instruction);
            var completionUpdates = this._client.CompleteChatStreamingAsync(chatMessages);
            await foreach (var completionUpdate in completionUpdates)
            {
                if (completionUpdate.ContentUpdate.Count > 0)
                {
                    var response = completionUpdate.ContentUpdate[0].Text;
                    yield return response;
                }
            }
        }

        List<ChatMessage> GetChatMessages(string[] messages, string instruction = null)
        {
            if (messages.None())
            {
                throw new ArgumentException("There are no messages");
            }

            var chatMessages = new List<ChatMessage>();
            if (instruction.HasValue())
            {
                chatMessages.Add(new SystemChatMessage(instruction));
            }
            chatMessages.AddRange(messages.Select(x => new UserChatMessage(x)));
            return chatMessages;
        }

    }
}