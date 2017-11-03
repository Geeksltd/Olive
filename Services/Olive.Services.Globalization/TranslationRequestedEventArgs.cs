namespace Olive.Services.Globalization
{
    using System;
    using System.ComponentModel;

    public class TranslationRequestedEventArgs : CancelEventArgs
    {
        public string PhraseInDefaultLanguage { get; internal set; }
        public ILanguage Language { get; internal set; }

        public Func<string> TranslationProvider;
    }
}
