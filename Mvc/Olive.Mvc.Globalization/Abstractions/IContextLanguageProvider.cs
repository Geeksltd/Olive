using System.Threading.Tasks;

namespace Olive.Globalization
{
    public interface IContextLanguageProvider
    {
        Task<ILanguage> GetCurrentLanguage();
    }
}
