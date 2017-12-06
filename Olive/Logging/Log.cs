using System;
using System.Collections.Generic;

namespace Olive
{
    public static class Log
    {
        static List<ILogger> Loggers = new List<ILogger>();

        static Log() => Loggers.Add(DefaultLogger.Instance);

        public static void RegisterLogger(ILogger logger) => Loggers.Add(logger);

        public static void ClearLogger() => Loggers.Clear();

        public static void Error(Exception ex) => Loggers.ForEach(logger => logger.RecordException(ex));

        public static void Error(string description, Exception ex = null)
        {
            if (ex == null) Record("Exception", description);
            else
                Loggers.ForEach(logger => logger.RecordException(description, ex));
        }

        public static void Warning(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Warning", description, relatedObject, userId, userIp);

        public static void Debug(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Debug", description, relatedObject, userId, userIp);

        public static void Info(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Info", description, relatedObject, userId, userIp);

        public static void Audit(string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Record("Audit", description, relatedObject, userId, userIp);

        public static void Record(string eventTitle, string description, object relatedObject = null, string userId = null, string userIp = null) =>
            Loggers.ForEach(logger => logger.Log(eventTitle, description, relatedObject, userId, userIp));
    }
}