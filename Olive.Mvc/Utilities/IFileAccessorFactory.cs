using System.Security.Principal;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public interface IFileAccessorFactory
    {
        Task<FileAccessor> Create(string path, IPrincipal currentUser);
    }
}