using System;

namespace Olive.Mvc
{
    class NLogger : ILogger
    {
        NLog.Logger Logger;

        public NLogger(NLog.Logger logger) => Logger = logger;

        public void Log(Exception ex) => Logger.Error(ex, ex.Message);

        public void Log(string description, Exception ex) => Logger.Error(ex, description);

        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            Logger.Info("Event Start\r\nTitle: '{0}', UserId: '{1}', UserIP: '{2}'", eventTitle, userId, userIp);
        }
    }
}