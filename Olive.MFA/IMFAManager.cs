namespace Olive.MFA
{
    using Olive.Email;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IMFAManager
    {
        Task SendOtpViaEmail(IEmailMessage emailMessage);
        Task<bool> SendOtpViaSms(string otp, string phoneNumber);
        string GetOtp();


    }
}
