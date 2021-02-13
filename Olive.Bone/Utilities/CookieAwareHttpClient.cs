using System.Net;
using System.Net.Http;

namespace Olive
{
    public class CookieAwareHttpClient : HttpClient
    {
        public CookieAwareHttpClient() : this(new CookieContainer()) { }

        public CookieAwareHttpClient(CookieContainer container)
            : base(new HttpClientHandler
            {
                CookieContainer = container,
                UseCookies = true,
                AllowAutoRedirect = true
            })
        {
            CookieContainer = container;

            DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
            DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en,en-GB;q=0.9");
        }

        public CookieContainer CookieContainer { get; private set; }
    }
}