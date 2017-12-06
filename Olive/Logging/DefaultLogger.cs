using System;

namespace Olive
{
    public class DefaultLogger : ILogger
    {
        static DefaultLogger InstanceField;

        public bool Enable { get; set; }

        public static DefaultLogger Instance
        {
            get
            {
                if (InstanceField == null)
                {
                    InstanceField = new DefaultLogger
                    {
                        Enable = Config.Get(key: "DebugMode", defaultValue: false)
                    };
                }

                return InstanceField;
            }
        }

        public DefaultLogger()
        {
            // single tone
        }

        public void Log(string eventTitle, string description, object relatedObject, string userId, string userIp)
        {
            if (!Enable) return;

            Console.Write($"Event Start\r\nTitle: '{eventTitle}', UserId: '{userId}', UserIP: '{userIp}'");
            Console.Write($"Description: {description}\r\nEvent End");
        }

        public void RecordException(Exception ex) => RecordException(string.Empty, ex);

        public void RecordException(string description, Exception ex)
        {
            if (!Enable) return;

            Console.Write(ex.ToLogString(description));
        }
    }
}
