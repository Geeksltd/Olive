using Olive.Entities;
using Olive.Globalization;
using System;
using System.Threading.Tasks;

namespace Olive
{
    public static class ContextGlobalizationExtensions
    {
        static ILanguage defaultLanguage;

        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// The source default language in which the application is programmed. Normally this is English.
        /// </summary>
        public static async Task<ILanguage> DefaultLanguage(this Context context)
        {
            if (defaultLanguage == null)
            {
                defaultLanguage = await Database.FirstOrDefault<ILanguage>(l => l.IsDefault);

                if (defaultLanguage == null)
                    throw new Exception("There is no default language specified in the system.");
            }

            return defaultLanguage;
        }

        /// <summary>
        /// Gets the current language of the context. For example this can be determined by the user's cookie.
        /// If no language is specified, then the default language will be used as configured in the database.
        /// </summary>
        public static Task<ILanguage> Language(this Context context)
        {
            return context.GetOptionalService<IContextLanguageProvider>()?.GetCurrentLanguage()
                 ?? context.DefaultLanguage();
        }
    }
}