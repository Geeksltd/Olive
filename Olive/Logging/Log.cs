using System;
using System.Collections.Generic;
using System.Text;

namespace Olive
{
    public static class Log
    {
        static List<ILogger> Loggers = new List<ILogger>();

        static Log()
        {
            if (Config.Get("Log:Console", defaultValue: false))
                Loggers.Add(new ConsoleLogger());

            if (Config.Get("Log:Debug", defaultValue: false))
                Loggers.Add(new DebugLogger());

            if (Config.Get("Log:File:IsActive", defaultValue: false))
                Loggers.Add(new FileLogger());
        }

        public static void RegisterLogger(ILogger logger) => Loggers.Add(logger);

        public static void ClearLogger() => Loggers.Clear();

        public static void Error(Exception ex) => Loggers.ForEach(logger => logger.Log(ex));

        public static void Error(string description, Exception ex = null)
        {
            if (ex == null) Record("Exception", description);
            else
                Loggers.ForEach(logger => logger.Log(description, ex));
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

        internal static string ToYaml(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            var r = new StringBuilder();
            r.AppendLine($"Event '{eventTitle}'");
            if (userId.HasValue()) r.AppendLine($"  UserId: {userId}");
            if (userIp.HasValue()) r.AppendLine($"  UserIP: {userIp}");
            if (relatedObject != null) r.AppendLine($"  Object: {{ {relatedObject.GetType()}: {relatedObject.ToStringOrEmpty()} }}");

            if (description.HasValue())
            {
                r.Append("  Description: ");
                var firstLine = true;
                foreach (var line in description.ToLines().Trim())
                {
                    if (!firstLine) r.Append("  Description: ".Length);
                    r.AppendLine(line);
                    firstLine = false;
                }
            }

            return r.ToString();
        }

    }
}