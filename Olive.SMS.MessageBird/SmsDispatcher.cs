using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Olive.SMS.MessageBird
{
    public class SmsDispatcher : ISmsDispatcher
    {
        public async Task Dispatch(ISmsMessage sms)
        {
            var accessKey = Config.GetOrThrow("Sms:MessageBird:AccessKey");
            var workspaceId = Config.GetOrThrow("Sms:MessageBird:WorkspaceId");
            var channelId = Config.GetOrThrow("Sms:MessageBird:ChannelId");

            var url = $"https://api.bird.com/workspaces/{workspaceId}/channels/{channelId}/messages";

            var payload = new
            {
                receiver = new
                {
                    contacts = new[]
                    {
                        new { identifierValue = sms.To }
                    }
                },
                body = new
                {
                    type = "text",
                    text = new
                    {
                        text = sms.Text
                    }
                }
            };

            var success = await new ApiClient(url)
                .Header(h => h.Authorization = new AuthenticationHeaderValue("AccessKey", accessKey))
                .Post(payload);

            if (!success)
                throw new Exception("Failed to send SMS via MessageBird.");
        }
    }
}
