using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Security
{

    class ProductionDataProtector : IDataProtector
    {

        // Purposes : v2,Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware,Cookies

        IDataProtector DataProtector;
        static Dictionary<string, Func<string, IDataProtector>> DataProtectorFactories = new Dictionary<string, Func<string, IDataProtector>>();

        public ProductionDataProtector()
        {
            DataProtector = new SymmetricKeyDataProtector("Default", Config.Get("Encryption:Cookie:Key"));
        }

        public ProductionDataProtector(string purpose)
        {
            var factory = DataProtectorFactories.GetOrDefault(purpose);
            if (factory != null)
                DataProtector = factory(purpose);
        }

        internal static void Register(string purpose, Func<string, IDataProtector> dataProtectorFactory) =>
            DataProtectorFactories.Add(purpose, dataProtectorFactory);

        public IDataProtector CreateProtector(string purpose) => new ProductionDataProtector(purpose);
        public byte[] Protect(byte[] plaintext) => DataProtector.Protect(plaintext);
        public byte[] Unprotect(byte[] protectedData) => DataProtector.Unprotect(protectedData);

    }
}
