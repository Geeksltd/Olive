using Olive.Entities;

namespace Olive.Globalization
{
    public interface IPhraseTranslation : IEntity
    {
        string Phrase { get; }
        string Translation { get; }
        ILanguage Language { get; }
    }
}
