using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IEmailFailureService
    {
        Task Check();
    }
}