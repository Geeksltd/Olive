namespace Olive
{
    using System;

    public interface ILogger
    {
        void Log(Exception ex);
        void Log(string description, Exception ex);
        void Log(string eventTitle, string description, object relatedObject, string userId, string userIp);
    }
}