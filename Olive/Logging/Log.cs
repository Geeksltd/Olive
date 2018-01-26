using NLog.Web;
using System;
using System.Collections.Generic;

namespace Olive
{
    public static class Log
    {
        public static string EmailRepositoryName = "EmailRepository";
        public static string EmailAppendarName = "EmailAppender";
        public static string EmailLoggerName = "EmailLogger";

        static List<NLog.ILogger> Loggers = new List<NLog.ILogger>();

        static Log()
        {
            Loggers.Add(NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger());
        }

        public static void RegisterLogger(NLog.ILogger logger) => Loggers.Add(logger);

        public static void ClearLogger() => Loggers.Clear();

        public static void Error(Exception ex) => Loggers.ForEach(logger => logger.Error(ex));

        public static void Error(string description, Exception ex = null)
        {
            if (ex == null)
                Record("Exception", description);
            else
                Loggers.ForEach(logger => logger.Error(ex, description));
        }

        public static void Warning(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Warning", description, relatedObject, userId, userIp);

        public static void Debug(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Debug", description, relatedObject, userId, userIp);

        public static void Info(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Info", description, relatedObject, userId, userIp);

        public static void Audit(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Audit", description, relatedObject, userId, userIp);

        public static void Record(string eventTitle, string description, object relatedObject = null, string userId = null, string userIp = null)
        {
            Loggers.ForEach(logger => logger.Info("Event Start\r\nTitle: '{0}', UserId: '{1}', UserIP: '{2}'", eventTitle, userId, userIp));
            Loggers.ForEach(logger => logger.Info("Description: {0}\r\nEvent End", description));
        }
    }
}