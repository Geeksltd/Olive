using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Olive
{
    public class AllMicroservices
    {
        public static string[] GetNames()
        {
            return Config
                .GetSection("Microservice")
                .GetChildren() 
                .Select(x => x.GetValue<string>("Name").Or(x.Key))
                .ExceptNull()
                .Distinct()
                .Cast<string>()
                .ToArray();
        }

        public static Microservice[] GetServices()
        {
            return GetNames()
              .Select(x => Microservice.Of(x))
              .ToArray();
        }
    }
}