using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Olive.Entities;
using Olive.Web;

namespace Olive.Globalization
{
    /// <summary>
    /// Provides translation services.
    /// </summary>
    public static class Translator
    {
        /// <summary>Length of the query without the phrase</summary>
        static readonly int GOOGLE_TRANSLATE_QUERY_LENGTH = 115;

        /// <summary>Maximum number of characters for each request to Google API</summary>
        static readonly int GOOGLE_TRANSLATE_LIMIT = 2000;

        /// <summary>Maximum number of characters for each phrase that can be sent to Google Translate</summary>
        public static readonly int GOOGLE_PHRASE_LIMIT = GOOGLE_TRANSLATE_LIMIT - GOOGLE_TRANSLATE_QUERY_LENGTH;

        /// <summary>Message returned by Google if suspected terms of service abuse.</summary>
        const string GOOGLE_TERMS_OF_SERVICE_ABUSE_MESSAGE = "Suspected Terms of Service Abuse. Please see http://code.google.com/apis/errors";

        /// <summary>HTML tag for a line break</summary>
        static readonly string LINE_BREAK_HTML = "<br />";

        /// <summary>Unicode value of a HTML line break</summary>
        static readonly string LINE_BREAK_UNICODE = "\u003cbr /\u003e";

        public static bool AttemptAutomaticTranslation = true;
        static bool IsGoogleTranslateMisconfigured;

        /// <summary>
        /// Gets the language of the current user from cookie.
        /// If no language is specified, then the default language will be used as configured in the database.
        /// </summary>
        public static Func<Task<ILanguage>> GetCurrentLanguage = async () => await CookieProperty.Get<ILanguage>() ?? await GetDefaultLanguage();

        static ILanguage DefaultLanguage;
        static async Task<ILanguage> GetDefaultLanguage()
        {
            if (DefaultLanguage == null)
            {
                DefaultLanguage = await Entity.Database.FirstOrDefault<ILanguage>(l => l.IsDefault);

                if (DefaultLanguage == null)
                {
                    throw new Exception("There is no default language specified in the system.");
                }
            }

            return DefaultLanguage;
        }

        #region Translate Html

        public static Task<string> TranslateHtml(string htmlInDefaultLanguage)
            => TranslateHtml(htmlInDefaultLanguage, null);

        public static async Task<string> TranslateHtml(string htmlInDefaultLanguage, ILanguage language)
        {
            if (language == null) language = await GetCurrentLanguage();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlInDefaultLanguage);

            var docNode = htmlDoc.DocumentNode;
            await TranslateNode(docNode, language);

            return docNode.OuterHtml;
        }

        static async Task TranslateNode(HtmlNode node, ILanguage language)
        {
            if (node.InnerHtml.Length == 0 ||
                (node.NodeType == HtmlNodeType.Text &&
                !Regex.IsMatch(node.InnerHtml, @"\w+" /* whitespaces */, RegexOptions.Multiline)))
                return;

            if (node.Name == "img")
            {
                var alt = node.Attributes["alt"];
                if (alt != null)
                    alt.Value = await Translate(alt.Value, language);
            }

            if (!node.HasChildNodes && node.InnerHtml.Length <= GOOGLE_TRANSLATE_LIMIT)
            {
                node.InnerHtml = await Translate(node.InnerHtml, language);
                return;
            }
            else if (node.ChildNodes.Count > 0)
            {
                foreach (var child in node.ChildNodes)
                    await TranslateNode(child, language);
            }
            else
            {
                var lines = Wrap(node.InnerHtml, GOOGLE_TRANSLATE_LIMIT);
                var sb = new StringBuilder();

                foreach (var line in lines)
                    sb.Append(await Translate(line, language));

                node.InnerHtml = sb.ToString();
                return;
            }
        }

        static string[] Wrap(string text, int eachLineLength)
        {
            text = text.Replace("\n\r", "\n");
            var splites = new[] { '\n', ' ', '.', ',', ';', '!', '?' };

            var resultLines = new List<string>();

            var currentLine = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (currentLine.Length <= eachLineLength)
                {
                    currentLine.Append(text[i]);
                }
                else // currentLineLength > eachLineLength
                {
                    while (!splites.Contains(currentLine[currentLine.Length - 1])/* last char is not splitter*/)
                    {
                        currentLine.Remove(currentLine.Length - 1, 1); // remove last char
                        i--;
                    }

                    i--;
                    resultLines.Add(currentLine.ToString());
                    currentLine = new StringBuilder();
                }
            }

            return resultLines.ToArray();
        }

        #endregion

        public static async Task<string> Translate(string phraseInDefaultLanguage)
        {
            var retries = 3;
            while (true)
            {
                try
                {
                    return await Translate(phraseInDefaultLanguage, null);
                }
                catch
                {
                    if (retries == 0) throw;

                    await Task.Delay(10); // Wait and try again:
                    retries--;
                }
            }
        }

        /// <summary>
        /// Occurs when a translation is requested.
        /// </summary>
        public static readonly AsyncEvent<TranslationRequestedEventArgs> TranslationRequested =
            new AsyncEvent<TranslationRequestedEventArgs>();

        [EscapeGCop("It is ok for try methods to have out parameters.")]
        static async Task<Tuple<bool, string>> TryTranslateUsingTheEvent(string phraseInDefaultLanguage, ILanguage language)
        {
            if (TranslationRequested.IsHandled())
            {
                var args = new TranslationRequestedEventArgs { PhraseInDefaultLanguage = phraseInDefaultLanguage, Language = language };

                await TranslationRequested.Raise(args);

                if (args.Cancel)
                    return Tuple.Create(true, phraseInDefaultLanguage);

                if (args.TranslationProvider != null)
                    return Tuple.Create(true, args.TranslationProvider());
            }

            return Tuple.Create(false, default(string));
        }

        public static async Task<string> Translate(string phraseInDefaultLanguage, ILanguage language)
        {
            if (language == null) language = await GetCurrentLanguage();

            var byEvent = await TryTranslateUsingTheEvent(phraseInDefaultLanguage, language);
            if (byEvent.Item1) return byEvent.Item2;

            if (phraseInDefaultLanguage.IsEmpty())
                return phraseInDefaultLanguage;

            if (language.Equals(await GetDefaultLanguage()))
            {
                return phraseInDefaultLanguage;
            }
            else
            {
                // First try: Exact match:
                var translation = await GetLocalTranslation(phraseInDefaultLanguage, language);

                if (translation.HasValue()) return translation;

                // special characters aren't translated:
                if (phraseInDefaultLanguage.ToCharArray().None(c => char.IsLetter(c)))
                    return phraseInDefaultLanguage;

                // Next try: Remove special characters:
                var leftDecorators = FindLeftDecorators(phraseInDefaultLanguage);
                var rightDecorators = FindRightDecorators(phraseInDefaultLanguage);

                if (leftDecorators.HasValue())
                    phraseInDefaultLanguage = phraseInDefaultLanguage.TrimStart(leftDecorators);

                if (rightDecorators.HasValue())
                    phraseInDefaultLanguage = phraseInDefaultLanguage.TrimEnd(rightDecorators);

                translation = await GetLocalTranslation(phraseInDefaultLanguage, language);

                if (translation.IsEmpty())
                {
                    if (phraseInDefaultLanguage.Length <= GOOGLE_TRANSLATE_LIMIT && AttemptAutomaticTranslation)
                    {
                        translation = await GoogleTranslate(phraseInDefaultLanguage, language.IsoCode);
                    }
                    else
                    {
                        translation = phraseInDefaultLanguage;
                    }

                    if (translation.HasValue())
                    {
                        try
                        {
                            var arg = new TranslationDownloadedEventArgs(phraseInDefaultLanguage, language, translation);
                            await TranslationDownloaded.Raise(arg);
                        }
                        catch { /* No Logging needed*/ }
                    }
                }

                return leftDecorators + translation.Or(phraseInDefaultLanguage) + rightDecorators;
            }
        }

        static async Task<string> GetLocalTranslation(string phraseInDefaultLanguage, ILanguage language)
        {
            return (await Entity.Database.FirstOrDefault<IPhraseTranslation>(p =>
                    p.Phrase == phraseInDefaultLanguage && p.Language.Equals(language))
                )
                .Get(p => p.Translation);
        }

        /// <summary>
        /// Occurs when a word's translation is downloaded off the Internet.
        /// </summary>
        public static readonly AsyncEvent<TranslationDownloadedEventArgs> TranslationDownloaded =
            new AsyncEvent<TranslationDownloadedEventArgs>();

        static string FindLeftDecorators(string phraseInDefaultLanguage)
        {
            var result = new StringBuilder();

            for (int i = 0; i < phraseInDefaultLanguage.Length && !char.IsLetter(phraseInDefaultLanguage[i]); i++)
                result.Append(phraseInDefaultLanguage[i]);

            return result.ToString();
        }

        static string FindRightDecorators(string phraseInDefaultLanguage)
        {
            var result = new StringBuilder();

            for (int i = phraseInDefaultLanguage.Length - 1; i >= 0 && !char.IsLetter(phraseInDefaultLanguage[i]); i--)
                result.Insert(0, phraseInDefaultLanguage[i]);

            return result.ToString();
        }

        /// <summary>Check the configuration status of Google Translate</summary>
        public static bool IsGoogleMisconfigured() => IsGoogleTranslateMisconfigured;

        /// <summary>Set the status of Google Translate as well configured</summary>
        public static void ReconfigureGoogleTranslate() => IsGoogleTranslateMisconfigured = false;

        /// <summary>
        /// Uses Google Translate service to translate a specified phrase to the specified language.
        /// </summary>
        public static async Task<string> GoogleTranslate(string phrase, string languageIsoCodeTo, string languageIsoCodeFrom = "en")
        {
            if (IsGoogleTranslateMisconfigured) return null;

            if (Config.Get("Enable.Google.Translate", defaultValue: false) == false) return null;

            var key = Config.Get<string>("Google.Translate.Key");
            if (key.IsEmpty())
                throw new InvalidOperationException("There is no key specified for Google Translate.");

            // Replace line breaks by HTML tag, otherwise the API will remove lines
            phrase = phrase.Replace(Environment.NewLine, LINE_BREAK_HTML);

            var request = "https://www.googleapis.com/language/translate/v2?key={0}&q={1}&source={2}&target={3}".FormatWith(key, HttpUtility.UrlEncode(phrase), languageIsoCodeFrom.ToLower(), languageIsoCodeTo.ToLower());
            if (request.Length > GOOGLE_TRANSLATE_LIMIT)
                throw new ArgumentOutOfRangeException("Cannot use google translate with queries larger than {0} characters".FormatWith(GOOGLE_TRANSLATE_LIMIT));

            try
            {
                var response = (await new WebClient().DownloadDataTaskAsync(request)).ToString(Encoding.UTF8);

                if (response.Contains(GOOGLE_TERMS_OF_SERVICE_ABUSE_MESSAGE, caseSensitive: false))
                {
                    IsGoogleTranslateMisconfigured = true;
                    return null;
                }
                else
                {
                    var ser = new DataContractJsonSerializer(typeof(GoogleTranslateJsonResponseRootObject));
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                    var rootObjectResponse = ser.ReadObject(stream) as GoogleTranslateJsonResponseRootObject;
                    var result = rootObjectResponse.data.translations[0].translatedText;
                    result = result.Replace(LINE_BREAK_UNICODE, Environment.NewLine);   // Decode line breaks
                    return HttpUtility.HtmlDecode(result);
                }
            }
            catch
            {
                // No  Logging needed
                return null;
            }
        }

        /// <summary>
        /// Detect the language of a phrase.
        /// The API can translate multiple piece of text in the same time, if needed create a function with parameter "params string phrase" and return a list of GoogleAutoDetectLanguage.
        /// </summary>
        public static async Task<GoogleAutodetectResponse> GoogleAutodetectLanguage(string phrase)
        {
            if (IsGoogleTranslateMisconfigured) return null;

            if (!Config.Get<bool>("Enable.Google.Autodetect", defaultValue: false))
                return null;

            var key = Config.Get<string>("Google.Translate.Key");
            if (key.IsEmpty())
                throw new InvalidOperationException("There is no key specified for Google Translate.");

            var request = "https://www.googleapis.com/language/translate/v2/detect?key={0}&q={1}".FormatWith(key, HttpUtility.UrlEncode(phrase));
            if (request.Length > GOOGLE_TRANSLATE_LIMIT)
                throw new ArgumentOutOfRangeException("Cannot use google translate with queries larger than {0} characters".FormatWith(GOOGLE_TRANSLATE_LIMIT));

            try
            {
                var response = Encoding.UTF8.GetString(await new WebClient().DownloadDataTaskAsync(request));

                if (response.Contains(GOOGLE_TERMS_OF_SERVICE_ABUSE_MESSAGE, caseSensitive: false))
                {
                    IsGoogleTranslateMisconfigured = true;
                    return null;
                }
                else
                {
                    var ser = new DataContractJsonSerializer(typeof(GoogleAutoDetectJsonResponseRootObject));
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                    var rootObjectResponse = ser.ReadObject(stream) as GoogleAutoDetectJsonResponseRootObject;
                    var dectection = rootObjectResponse.data.detections[0][0];
                    return new GoogleAutodetectResponse(dectection.language, dectection.confidence);
                }
            }
            catch
            {
                // No Logging needed
                return null;
            }
        }
    }
}