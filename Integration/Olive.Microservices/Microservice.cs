using System.Net.Http;

namespace Olive
{
    /// <summary>
    /// Provides helper services for implementing microservices using Olive.
    /// </summary>
    public class Microservice
    {
        public string Name { get; private set; }
        string BaseUrl, AccessKey;

        Microservice(string name) => Name = name;

        public static Microservice Of(string serviceName)
        {
            return new Microservice(serviceName)
            {
                BaseUrl = Config.GetOrThrow("Microservice:" + serviceName + ":Url").EnsureEndsWith("/"),
                AccessKey = Config.Get("Microservice:" + serviceName + ":AccessKey")
            };
        }

        public static Microservice Me
        {
            get
            {
                var url = Config.Get("Microservice:Me:Url");
                if (url.IsEmpty()) url = "Config value not specified for 'Microservice:Me:Url'";
                else url = url.EnsureEndsWith("/");

                return new Microservice(Config.GetOrThrow("Microservice:Me:Name")) { BaseUrl = url };
            }
        }

        /// <summary>
        /// Returns the full url to a specified resource in this microservice by
        /// concatinating the base url of this service with the specified relative url.
        /// </summary>
        public string Url(string relativeUrl = null) => BaseUrl + relativeUrl.OrEmpty().TrimStart("/");

        /// <summary>
        /// Creates an Api client for this service.
        /// It will automatically add an authentication cookie based on the service key.
        /// </summary>
        public ApiClient Api(string relativeApiUrl)
        {
            var result = new ApiClient(Url(relativeApiUrl));

            result.Header(x => x.Add("Microservice.AccessKey", AccessKey));

            return result;
        }
    }
}