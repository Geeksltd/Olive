namespace Olive.MFA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITemporaryLoginService
    {
        Task DeleteTemporaryLogin(ITemporaryLogin login);
        Task<bool> IsSessionExpired(ITemporaryLogin login);
        Task<bool> IsOtpValid(ITemporaryLogin login, string code);
    }
}
