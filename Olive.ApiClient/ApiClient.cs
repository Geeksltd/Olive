using System;

namespace Olive
{
    public partial class ApiClient
    {
        public static Func<string> GetSessionToken = () =>
        {
            var filName = "SessionToken.txt";
            if (System.IO.File.Exists(filName))
                return System.IO.File.ReadAllText(filName);
            return null;
        };
    }

    internal class ServerError
    {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
    }
}