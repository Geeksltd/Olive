using System;
using System.Net.Http;

namespace Olive.Mvc
{
    public class RecaptchaOptions
    {
        public string ResponseValidationEndpoint { get; set; } = RecaptchaDefaults.ResponseValidationEndpoint;

        public string JavaScriptUrl { get; set; } = RecaptchaDefaults.JavaScriptUrl;

        public string SiteKey { get; set; }

        public bool Enabled { get; set; } = true;

        public string SecretKey { get; set; }

        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        public TimeSpan BackchannelTimeout { get; set; } = 60.Seconds();

        public RecaptchaControlSettings ControlSettings { get; set; } = new RecaptchaControlSettings();

        public string ValidationMessage { get; set; }

        public string LanguageCode { get; set; }
    }
}
