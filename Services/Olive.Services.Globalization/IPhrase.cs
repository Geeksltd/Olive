using Olive.Entities;

namespace Olive.Services.Globalization
{
    public interface IPhraseTranslation : IEntity
    {
        string Phrase { get; }
        string Translation { get; }
        ILanguage Language { get; }
    }
}
