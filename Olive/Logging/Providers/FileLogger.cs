using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Olive
{
    class FileLogger : ILogger
    {
        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            write($"Event Start\r\nTitle: '{eventTitle}', UserId: '{userId}', UserIP: '{userIp}'");
            write($"Description: {description}\r\nEvent End");
        }

        public void Log(Exception ex) => Log(string.Empty, ex);

        public void Log(string description, Exception ex) => write(ex.ToLogString(description));

        private void write(string msg)
        {
            using (StreamWriter w = File.AppendText(Config.Get("Log:File:Path", defaultValue: "information.log")))
            {
                w.WriteLine(msg);
            }
        }
    }
}
