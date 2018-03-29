using System;
using System.Diagnostics;

namespace Olive
{
    class DebugLogger : ILogger
    {
        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            Debug.WriteLine(Olive.Log.ToYaml(eventTitle, description, relatedObject, userId, userIp));
        }

        public void Log(Exception ex) => Log(string.Empty, ex);

        public void Log(string description, Exception ex) => Debug.Write(ex.ToLogString(description));
    }
}
