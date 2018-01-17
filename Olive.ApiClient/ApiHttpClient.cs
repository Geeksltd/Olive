using System;
using System.Linq;

namespace Olive.ApiClient
{
    public partial class ApiHttpClient
    {
        public static string BaseUrl = Config.Get("Api.Base.Url").OrEmpty().TrimEnd("/");

        public static Func<string> GetSessionToken = () =>
        {
            string filName = "SessionToken.txt";
            if (System.IO.File.Exists(filName))
                return System.IO.File.ReadAllText(filName);
            return null;
        };

        /// <summary>
        /// Returns a full absolute URL for a specified relativeUrl.
        /// </summary>
        public static string Url(params string[] relativeUrlParts)
        {
            var result = relativeUrlParts.Trim().Select(x => x.TrimStart("/").TrimEnd("/")).Trim().ToString("/");

            if (result.StartsWithAny("http://", "https://")) return result;

            if (BaseUrl.LacksAll()) throw new Exception("Could not find the config value for 'Api.Base.Url'.");

            return BaseUrl.EnsureEndsWith("/") + result;
        }
    }

    internal class ServerError
    {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
    }
}