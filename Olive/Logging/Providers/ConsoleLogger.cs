using System;

namespace Olive
{
    class ConsoleLogger : ILogger
    {
        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            Console.WriteLine(Olive.Log.ToYaml(eventTitle, description, relatedObject, userId, userIp));
        }

        public void Log(Exception ex) => Log(string.Empty, ex);

        public void Log(string description, Exception ex) => Console.Write(ex.ToLogString(description));
    }
}
