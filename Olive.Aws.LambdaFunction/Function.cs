using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Olive.Aws
{
    public abstract class Function<TStartup> where TStartup : Startup, new()
    {
        static bool FirstCall = true;

        protected IConfiguration Configuration { get; private set; }

        protected Function()
        {
            if (FirstCall)
            {
                FirstCall = false;
                new TStartup();
            }
        }

        public abstract Task ExecuteAsync(ILambdaContext context);

        public ILogger Log => Olive.Log.For(this);
    }
}