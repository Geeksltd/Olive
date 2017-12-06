namespace Olive.Services.Globalization
{
    public class TranslationRequestedEventArgs : CancelEventArgs
    {
        public string PhraseInDefaultLanguage { get; internal set; }
        public ILanguage Language { get; internal set; }

        public Func<string> TranslationProvider;
    }
}
