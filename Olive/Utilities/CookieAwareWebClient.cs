using System;
using System.Net;

namespace Olive
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieAwareWebClient() : this(new CookieContainer()) { }

        public CookieAwareWebClient(CookieContainer container) => CookieContainer = container;

        public CookieContainer CookieContainer { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);

            if (result is HttpWebRequest castRequest)
                castRequest.CookieContainer = CookieContainer;

            return result;
        }
    }
}