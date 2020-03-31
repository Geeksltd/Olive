using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Globalization
{
    public partial class Translator
    {
        public readonly static List<ITranslationProvider> Providers = new List<ITranslationProvider>();

        static IDatabase Database => Context.Current.Database();

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
        public static event AwaitableEventHandler<TranslationDownloadedEventArgs> TranslationDownloaded;

        public static async Task<string> Translate(string phraseInDefaultLanguage)
            => await Translate(phraseInDefaultLanguage, await Context.Current.Language());

        public static async Task<string> Translate(string phraseInDefaultLanguage, ILanguage language)
        {
            if (language == null) throw new ArgumentNullException(nameof(language));

            if (phraseInDefaultLanguage.IsEmpty()) return phraseInDefaultLanguage;
            if (language.Equals(await Context.Current.DefaultLanguage())) return phraseInDefaultLanguage;

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
                    translation = await translator.Translate(phraseInDefaultLanguage,
                        (await Context.Current.DefaultLanguage()).IsoCode, language.IsoCode);

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
            var record = await Database.FirstOrDefault<IPhraseTranslation>(p =>
                    p.Phrase == phraseInDefaultLanguage && p.Language == language);

            return record?.Translation;
        }
    }
}