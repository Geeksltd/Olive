using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Olive
{
    class FileLogger : ILogger
    {
        static FileInfo LogFile
        {
            get
            {
                var file = Config.Get("Log:File:Path", defaultValue: "log.txt");
                return AppDomain.CurrentDomain.GetBaseDirectory().GetFile(file);
            }
        }

        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            LogFile.AppendAllText(Olive.Log.ToYaml(eventTitle, description, relatedObject, userId, userIp));
        }

        public void Log(Exception ex) => Log(string.Empty, ex);

        public void Log(string description, Exception ex) => LogFile.AppendAllText(ex.ToLogString(description));
    }
}
