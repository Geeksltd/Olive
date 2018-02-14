using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Globalization
{
    public interface IContextLanguageProvider
    {
        Task<ILanguage> GetCurrentLanguage();
    }
}
