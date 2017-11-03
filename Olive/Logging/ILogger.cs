namespace Olive
{
    using System;

    public interface ILogger
    {
        void RecordException(Exception ex);
        void RecordException(string description, Exception ex);
        void Log(string eventTitle, string description, object relatedObject, string userId, string userIp);
    }
}