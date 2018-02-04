using System.Net;
using System.Net.Http;

namespace Olive
{
    public class CookieAwareHttpClient : HttpClient
    {
        public CookieAwareHttpClient() : this(new CookieContainer()) { }

        public CookieAwareHttpClient(CookieContainer container)
            : base(new HttpClientHandler() { CookieContainer = container })
        {
            CookieContainer = container;
        }

        public CookieContainer CookieContainer { get; set; }
    }
}