using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Olive
{
    /*
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
    */

    public class CookieAwareHttpClient : HttpClient
    {
        public CookieAwareHttpClient() : this(new CookieContainer()) { }

        public CookieAwareHttpClient(CookieContainer container)
            : base(new HttpClientHandler() { CookieContainer = container }) // Initialized the HttpClient With Custom CookieContainer
        {
            CookieContainer = container;
        }

        public CookieContainer CookieContainer { get; set; }

    }
}