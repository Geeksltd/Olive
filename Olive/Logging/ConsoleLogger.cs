using System;

namespace Olive
{
    class ConsoleLogger : ILogger
    {
        public bool Enable { get; set; } = Config.Get("DebugMode", defaultValue: false);

        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            if (!Enable) return;

            Console.Write($"Event Start\r\nTitle: '{eventTitle}', UserId: '{userId}', UserIP: '{userIp}'");
            Console.Write($"Description: {description}\r\nEvent End");
        }

        public void Log(Exception ex) => Log(string.Empty, ex);

        public void Log(string description, Exception ex)
        {
            if (!Enable) return;

            Console.Write(ex.ToLogString(description));
        }
    }
}
