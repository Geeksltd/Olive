namespace Olive.MFA
{
    using Olive.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TemporaryLoginService : ITemporaryLoginService
    {
        IDatabase Database => Context.Current.Database();
        public Task DeleteTemporaryLogin(ITemporaryLogin login)
        {
            return Database.Delete(login);
        }

        public async Task<bool> IsOtpValid(ITemporaryLogin login, string code)
        {
           return login.MFACode.HasValue() && login.CreatedAt.AddMinutes((double)login.ExpiryMinutes) > LocalTime.Now && 
                login.MFACode.Equals(code);
        }

        public async Task<bool> IsSessionExpired(ITemporaryLogin login)
        {
            return login.CreatedAt.AddMinutes(login.ExpiryMinutes) < LocalTime.Now;
        }
    }
}
