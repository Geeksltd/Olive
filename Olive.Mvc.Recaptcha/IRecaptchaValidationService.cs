using System.Threading.Tasks;

namespace Olive.Mvc
{
    public interface IRecaptchaValidationService
    {
        Task ValidateResponseAsync(string response, string remoteIp);

        string ValidationMessage { get; }
    }
}
