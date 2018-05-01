using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Olive.Globalization
{
    /// <summary>
    /// Provides translation services.
    /// </summary>
    public class GoogleTranslationProvider : ITranslationProvider
    {
        /// <summary>
        /// Check the configuration status of Google Translate/
        /// </summary>
        public static bool IsMisconfigured { get; private set; }

        static string ApiKey => Config.GetOrThrow("Globalization:GoogleTranslate:ApiKey");
        static int MaxQueryLength => Config.Get("Globalization:GoogleTranslate:QueryLength", defaultValue: 115);
        static int RequestCharacterLimit => Config.Get("Globalization:GoogleTranslate:CharacterLimit", defaultValue: 2000);

        static string SuspectedAbuseMessage
            => Config.Get("Globalization:GoogleTranslate:SuspectedAbuseMessage", "Suspected Terms of Service Abuse.");

        public int MaximumTextLength => RequestCharacterLimit - MaxQueryLength;

        static readonly string LINE_BREAK_UNICODE = "\u003cbr /\u003e";

        /// <summary>Set the status of Google Translate as well configured</summary>
        public static void Reconfigure() => IsMisconfigured = false;

        /// <summary>
        /// Uses Google Translate service to translate a specified phrase to the specified language.
        /// </summary>
        public async Task<string> Translate(string phrase, string languageIsoCodeTo, string languageIsoCodeFrom = "en")
        {
            if (IsMisconfigured) return null;

            // Replace line breaks by HTML tag, otherwise the API will remove lines
            phrase = phrase.Replace(Environment.NewLine, "<br/>");

            var request = "https://www.googleapis.com/language/translate/v2?key={0}&q={1}&source={2}&target={3}".FormatWith(ApiKey, HttpUtility.UrlEncode(phrase), languageIsoCodeFrom.ToLower(), languageIsoCodeTo.ToLower());
            if (request.Length > RequestCharacterLimit)
                throw new ArgumentOutOfRangeException("Cannot use google translate with queries larger than {0} characters".FormatWith(RequestCharacterLimit));

            try
            {
                var response = (await new HttpClient().DownloadData(request, handleGzip: true))
                    .ToString(Encoding.UTF8);

                if (response.Contains(SuspectedAbuseMessage, caseSensitive: false))
                {
                    IsMisconfigured = true;
                    return null;
                }

                var ser = new DataContractJsonSerializer(typeof(GoogleTranslateJsonResponseRootObject));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
                {
                    if (ser.ReadObject(stream) is GoogleTranslateJsonResponseRootObject rootObjectResponse)
                    {
                        var result = rootObjectResponse.data.translations[0].translatedText;
                        result = result.Replace(LINE_BREAK_UNICODE, Environment.NewLine);   // Decode line breaks
                        return HttpUtility.HtmlDecode(result);
                    }
                }
            }
            catch
            {
                // No  Logging needed
                return null;
            }

            return null;
        }

        /// <summary>
        /// Detect the language of a phrase.
        /// The API can translate multiple piece of text in the same time, if needed create a function with parameter "params string phrase" and return a list of GoogleAutoDetectLanguage.
        /// </summary>
        public async Task<GoogleAutodetectResponse> AutodetectLanguage(string phrase)
        {
            if (IsMisconfigured) return null;

            var request = "https://www.googleapis.com/language/translate/v2/detect?key={0}&q={1}".FormatWith(ApiKey, HttpUtility.UrlEncode(phrase));
            if (request.Length > RequestCharacterLimit)
                throw new ArgumentOutOfRangeException("Cannot use google translate with queries larger than {0} characters".FormatWith(RequestCharacterLimit));

            try
            {
                var data = await new HttpClient().DownloadData(request, handleGzip: true);
                var response = Encoding.UTF8.GetString(data);

                if (response.Contains(SuspectedAbuseMessage, caseSensitive: false))
                {
                    IsMisconfigured = true;
                    return null;
                }

                var ser = new DataContractJsonSerializer(typeof(GoogleAutoDetectJsonResponseRootObject));
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                var rootObjectResponse = ser.ReadObject(stream) as GoogleAutoDetectJsonResponseRootObject;
                var dectection = rootObjectResponse?.data.detections[0][0];
                return new GoogleAutodetectResponse(dectection?.language, dectection?.confidence);
            }
            catch
            {
                // No Logging needed
                return null;
            }
        }
    }
}