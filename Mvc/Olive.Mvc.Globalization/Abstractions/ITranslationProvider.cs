using System.Threading.Tasks;

namespace Olive.Globalization
{
    public interface ITranslationProvider
    {
        Task<string> Translate(string phrase, string languageIsoCodeFrom, string languageIsoCodeTo);

        int MaximumTextLength { get; }
    }
}
