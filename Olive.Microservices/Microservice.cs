using System.Net.Http;

namespace Olive
{
    /// <summary>
    /// Provides helper services for implementing microservices using Olive.
    /// </summary>
    public class Microservice
    {
        public string Name { get; private set; }
        string BaseUrl, BaseResourceUrl, AccessKey;

        Microservice(string name) => Name = name;

        public static Microservice Of(string serviceName)
        {
            return new Microservice(serviceName)
            {
                BaseUrl = Config.GetOrThrow("Microservice:" + serviceName + ":Url").EnsureEndsWith("/"),
                BaseResourceUrl = Config.Get("Authentication:Cookie:Domain").EnsureStartsWith("https://").EnsureEndsWith("/")+serviceName.ToLower()+"/",
                AccessKey = Config.Get("Microservice:" + serviceName + ":AccessKey")
            };
        }

        public static Microservice Me
        {
            get
            {
                var url = Config.Get("Microservice:Me:Url");
                url = url.IsEmpty() ? "Config value not specified for 'Microservice:Me:Url'" : url.EnsureEndsWith("/");

                var name = Config.GetOrThrow("Microservice:Me:Name");

                return new Microservice(name)
                {
                    BaseUrl = url,
                    BaseResourceUrl = Config.Get("Authentication:Cookie:Domain").EnsureStartsWith("https://").EnsureEndsWith("/")+name.ToLower()+"/"
                };
            }
        }

        /// <summary>
        /// Returns the full url to a specified resource in this microservice by
        /// concatinating the base url of this service with the specified relative url.
        /// </summary>
        public string Url(string relativeUrl = null) => relativeUrl?.Contains("://")==true ? relativeUrl : BaseUrl + relativeUrl.OrEmpty().TrimStart("/");
        public string GetResourceUrl(string relativeUrl = null) => relativeUrl?.Contains("://")==true ? relativeUrl : BaseResourceUrl + relativeUrl.OrEmpty().TrimStart("/");

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