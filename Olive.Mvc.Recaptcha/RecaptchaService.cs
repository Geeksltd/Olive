using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace Olive.Mvc
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly RecaptchaOptions Options;
        private readonly HttpClient BackChannel;

        public RecaptchaService(IOptions<RecaptchaOptions> options, IConfiguration configuration)
        {
            Options = options.Value ?? new RecaptchaOptions
            {
                SiteKey = configuration["Recaptcha:SiteKey"],
                SecretKey = configuration["Recaptcha:SecretKey"]
            };

            ControlSettings = Options.ControlSettings ?? new RecaptchaControlSettings();

            BackChannel = new HttpClient(Options.BackchannelHttpHandler ?? new HttpClientHandler())
            {
                Timeout = Options.BackchannelTimeout
            };
        }

        public bool Enabled => Options.Enabled;

		public string SiteKey => Options.SiteKey;

        public string JavaScriptUrl => Options.JavaScriptUrl;

        public string ValidationMessage => 
            Options.ValidationMessage.Or("Validate that you are not a robot.");

        public string LanguageCode => Options.LanguageCode;

        public RecaptchaControlSettings ControlSettings { get; } 

        public async Task ValidateResponseAsync(string response, string remoteIp)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, RecaptchaDefaults.ResponseValidationEndpoint);
            var paramaters = new Dictionary<string, string>
            {
                ["secret"] = Options.SecretKey,
                ["response"] = response,
                ["remoteip"] = remoteIp
            };
            request.Content = new FormUrlEncodedContent(paramaters);

            var resp = await BackChannel.SendAsync(request);
            resp.EnsureSuccessStatusCode();

            var responseText = await resp.Content.ReadAsStringAsync();

            var validationResponse = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RecaptchaValidationResponse>(responseText));

            if (!validationResponse.Success)
            {
                throw new RecaptchaValidationException(GetErrrorMessage(validationResponse, out bool invalidResponse), invalidResponse);
            }
        }

        private string GetErrrorMessage(RecaptchaValidationResponse validationResponse, out bool invalidResponse)
        {
            var errorList = new List<string>();
            invalidResponse = false;

            if (validationResponse.ErrorCodes != null)
            {
                foreach (var error in validationResponse.ErrorCodes)
                {
                    switch (error)
                    {
                        case "missing-input-secret":
                            errorList.Add("The secret parameter is missing.");
                            break;
                        case "invalid-input-secret":
                            errorList.Add("The secret parameter is invalid or malformed.");
                            break;
                        case "missing-input-response":
                            errorList.Add("The response parameter is missing.");
                            invalidResponse = true;
                            break;
                        case "invalid-input-response":
                            errorList.Add("The response parameter is invalid or malformed.");
                            invalidResponse = true;
                            break;
                        default:
                            errorList.Add($"Unknown error '{error}'.");
                            break;
                    }
                }
            }
            else
            {
                return "Unspecified remote server error.";
            }

            return string.Join(Environment.NewLine, errorList);
        }
    }
}
