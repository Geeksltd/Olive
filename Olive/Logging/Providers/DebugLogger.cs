using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Olive
{
    class DebugLogger : ILogger
    {
        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            Debug.Write($"Event Start\r\nTitle: '{eventTitle}', UserId: '{userId}', UserIP: '{userIp}'");
            Debug.Write($"Description: {description}\r\nEvent End");
        }

        public void Log(Exception ex) => Log(string.Empty, ex);

        public void Log(string description, Exception ex) => Debug.Write(ex.ToLogString(description));
    }
}
