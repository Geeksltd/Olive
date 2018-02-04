using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Olive.Entities;
using Olive.Web;

namespace Olive.Globalization
{
    public partial class Translator
    {
        static ILanguage DefaultLanguage;
        public readonly static List<ITranslationProvider> Providers = new List<ITranslationProvider>();

        static Translator()
        {
            if (Config.Get("Globalization:GoogleTranslate:Enabled", defaultValue: false))
            {
                Providers.Add(new GoogleTranslationProvider());
            }
        }

        /// <summary>
        /// Occurs when a word's translation is downloaded off the Internet.
        /// </summary>
        public static readonly AsyncEvent<TranslationDownloadedEventArgs> TranslationDownloaded =
            new AsyncEvent<TranslationDownloadedEventArgs>();

        /// <summary>
        /// Gets the language of the current user from cookie.
        /// If no language is specified, then the default language will be used as configured in the database.
        /// </summary>
        public static Func<Task<ILanguage>> GetCurrentLanguage = async ()
            => await CookieProperty.Get<ILanguage>() ?? await GetDefaultLanguage();

        public static async Task<ILanguage> GetDefaultLanguage()
        {
            if (DefaultLanguage == null)
            {
                DefaultLanguage = await Entity.Database.FirstOrDefault<ILanguage>(l => l.IsDefault);

                if (DefaultLanguage == null)
                    throw new Exception("There is no default language specified in the system.");
            }

            return DefaultLanguage;
        }

        public static async Task<string> Translate(string phraseInDefaultLanguage)
            => await Translate(phraseInDefaultLanguage, await GetCurrentLanguage());

        public static async Task<string> Translate(string phraseInDefaultLanguage, ILanguage language)
        {
            if (language == null) throw new ArgumentNullException(nameof(language));

            if (phraseInDefaultLanguage.IsEmpty()) return phraseInDefaultLanguage;
            if (language.Equals(await GetDefaultLanguage())) return phraseInDefaultLanguage;

            // Already saved locally?
            var translation = await GetLocalTranslation(phraseInDefaultLanguage, language);
            if (translation.HasValue()) return translation;

            // special characters aren't translated:
            if (phraseInDefaultLanguage.ToCharArray().None(c => char.IsLetter(c)))
                return phraseInDefaultLanguage;

            // Clean up:
            var leftDecorators = FindLeftDecorators(phraseInDefaultLanguage);
            if (leftDecorators.HasValue()) phraseInDefaultLanguage = phraseInDefaultLanguage.TrimStart(leftDecorators);

            var rightDecorators = FindRightDecorators(phraseInDefaultLanguage);
            if (rightDecorators.HasValue()) phraseInDefaultLanguage = phraseInDefaultLanguage.TrimEnd(rightDecorators);

            // Try local translations again:
            translation = await GetLocalTranslation(phraseInDefaultLanguage, language);

            if (translation.IsEmpty())
            {
                foreach (var translator in Providers)
                {
                    translation = await translator.Translate(phraseInDefaultLanguage, DefaultLanguage.IsoCode, language.IsoCode);

                    if (translation.HasValue())
                    {
                        try
                        {
                            var arg = new TranslationDownloadedEventArgs(phraseInDefaultLanguage, language, translation);
                            await TranslationDownloaded.Raise(arg);
                        }
                        catch { /* No Logging needed*/ }
                        break;
                    }
                }
            }

            return leftDecorators + translation.Or(phraseInDefaultLanguage) + rightDecorators;
        }

        static string FindLeftDecorators(string phraseInDefaultLanguage)
        {
            var result = new StringBuilder();

            for (var i = 0; i < phraseInDefaultLanguage.Length && !char.IsLetter(phraseInDefaultLanguage[i]); i++)
                result.Append(phraseInDefaultLanguage[i]);

            return result.ToString();
        }

        static string FindRightDecorators(string phraseInDefaultLanguage)
        {
            var result = new StringBuilder();

            for (var i = phraseInDefaultLanguage.Length - 1; i >= 0 && !char.IsLetter(phraseInDefaultLanguage[i]); i--)
                result.Insert(0, phraseInDefaultLanguage[i]);

            return result.ToString();
        }

        static async Task<string> GetLocalTranslation(string phraseInDefaultLanguage, ILanguage language)
        {
            var record = await Entity.Database.FirstOrDefault<IPhraseTranslation>(p =>
                    p.Phrase == phraseInDefaultLanguage && p.Language == language);

            return record?.Translation;
        }
    }
}