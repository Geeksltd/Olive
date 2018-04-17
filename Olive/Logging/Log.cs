using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Olive
{
    public static class Log
    {
        /// <summary>
        /// A shortcut to Context.Current.GetService«ILogger»().
        /// </summary>
        public static ILogger Current => Context.Current.GetService<ILogger>();

        public static void Error(Exception ex, string message = null) => Current.LogError(ex, message);

        public static void Warning(string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            Current.LogWarning(ToYaml(message, relatedObject, userId, userIp));
        }

        public static void Debug(string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            Current.LogDebug(ToYaml(message, relatedObject, userId, userIp));
        }

        public static void Info(string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            Current.LogInformation(ToYaml(message, relatedObject, userId, userIp));
        }

        public static void Audit(string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            Current.LogTrace(ToYaml(message, relatedObject, userId, userIp));
        }

        static string ToYaml(string description, object relatedObject, string userId, string userIp)
        {
            var r = new StringBuilder();
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