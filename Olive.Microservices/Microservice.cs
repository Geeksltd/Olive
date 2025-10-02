using Microsoft.Extensions.Hosting;

namespace Olive
{
    public class Microservice
    {
        string _baseUrl, _baseResourceUrl, _baseS3BucketUrl, _accessKey;

        string BaseUrl =>
            _baseUrl ??= Config.GetOrThrow("Microservice:" + Name + ":Url").EnsureEndsWith("/");

        string BaseResourceUrl
        {
            get
            {
                if (_baseResourceUrl.HasValue()) return _baseResourceUrl;
                var isDevelopment = Context.Current.GetService<IHostEnvironment>().IsDevelopment();
                _baseResourceUrl = isDevelopment
                    ? BaseUrl
                    : Config.Get("Authentication:Cookie:Domain").EnsureStartsWith("https://").EnsureEndsWith("/") + Name.ToLower() + "/";
                return _baseResourceUrl;
            }
        }

        string BaseS3BucketUrl =>
            _baseS3BucketUrl ??= Config.Get("Microservice:" + Name + ":S3BucketUrl")
                .Or($"https://{Config.Get("Blob:S3:Bucket")}.s3.{Config.Get("Blob:S3:Region").Or(Config.Get("Aws:Region"))}.amazonaws.com/")
                .EnsureEndsWith("/");

        string AccessKey =>
            _accessKey ??= Config.Get("Microservice:" + Name + ":AccessKey");

        Microservice(string name, string url = null)
        {
            Name = name;
            _baseUrl = url;
        }

        public string Name { get; private set; }
        public string Icon => Config.Get("Microservice:" + Name + ":icon").Or(Config.Get("Microservice:" + Name + ":Icon"));
        public bool Sso => Config.Get("Microservice:" + Name + ":Sso")?.ToLower() == "true";
        public bool Iframe => Config.Get("Microservice:" + Name + ":Iframe")?.ToLower() == "true";

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

                return new Microservice(name, url);
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