using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public interface IValidateRecaptchaFilter
    {
        Task OnAuthorizationAsync(AuthorizationFilterContext context);
    }
}