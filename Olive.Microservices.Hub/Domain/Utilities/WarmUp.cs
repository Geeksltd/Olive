using System.Linq;
using System.Net.Http;

namespace Domain
{
    public class WarmUp
    {
        static HttpClient Client = new HttpClient();

        public static void PingAll()
        {
            Feature.All.Select(x => x.GetAbsoluteImplementationUrl())
                .AsParallel()
                .ForAll(Ping);
        }

        static void Ping(string url)
        {
            try { Client.GetStringAsync(url); }
            catch
            {
                // No logging is needed
            }
        }
    }
}