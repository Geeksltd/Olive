using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Olive
{
    /// <summary>
    /// Provides helper services for implementing microservices using Olive.
    /// </summary>
    public class Microservice
    {
        public string Name { get; private set; }
        string BaseUrl, BaseResourceUrl, BaseS3BucketUrl, AccessKey;

        Microservice(string name)
        {
            Name = name;

            BaseUrl = Config.GetOrThrow("Microservice:" + name + ":Url").EnsureEndsWith("/");
            
            var isDevelopment = Context.Current.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment();
            BaseResourceUrl = isDevelopment
                ? BaseUrl
                : Config.Get("Authentication:Cookie:Domain").EnsureStartsWith("https://").EnsureEndsWith("/") + name.ToLower() + "/";

            BaseS3BucketUrl =$"https://{Config.Get("Blob:S3:Bucket")}.s3.{Config.Get("Blob:S3:Region")}.amazonaws.com/"; ;

            AccessKey = Config.Get("Microservice:" + name + ":AccessKey");
        }

        public static Microservice Of(string serviceName)
        {
            return new Microservice(serviceName);
        }

        public static Microservice Me
        {
            get
            {
                var url = Config.Get("Microservice:Me:Url");
                url = url.IsEmpty() ? "Config value not specified for 'Microservice:Me:Url'" : url.EnsureEndsWith("/");

                var name = Config.GetOrThrow("Microservice:Me:Name");

                return new Microservice(name);
            }
        }

        /// <summary>
        /// Returns the full url to a specified resource in this microservice by
        /// concatinating the base url of this service with the specified relative url.
        /// </summary>
        public string Url(string relativeUrl = null) => relativeUrl?.Contains("://") == true ? relativeUrl : BaseUrl + relativeUrl.OrEmpty().TrimStart("/");
        public string GetResourceUrl(string relativeUrl = null) => relativeUrl?.Contains("://") == true ? relativeUrl : BaseResourceUrl + relativeUrl.OrEmpty().TrimStart("/");
        public string GetS3BucketUrl(string relativeUrl = null) => relativeUrl?.Contains("://") == true ? relativeUrl : BaseS3BucketUrl + relativeUrl.OrEmpty().TrimStart("/");

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