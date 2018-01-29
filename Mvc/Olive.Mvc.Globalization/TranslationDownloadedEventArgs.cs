using System;

namespace Olive.Globalization
{
    public class TranslationDownloadedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new TranslationDownloadedEventArgs instance.
        /// </summary>
        public TranslationDownloadedEventArgs(string word, ILanguage language, string translation)
        {
            Word = word;
            Language = language;
            Translation = translation;
        }

        #region Word
        /// <summary>
        /// Gets or sets the Word of this TranslationDownloadedEventArgs.
        /// </summary>
        public string Word { get; private set; }
        #endregion

        #region Language
        /// <summary>
        /// Gets or sets the Language of this TranslationDownloadedEventArgs.
        /// </summary>
        public ILanguage Language { get; private set; }
        #endregion

        #region Translation
        /// <summary>
        /// Gets or sets the Translation of this TranslationDownloadedEventArgs.
        /// </summary>
        public string Translation { get; private set; }
        #endregion
    }
}
