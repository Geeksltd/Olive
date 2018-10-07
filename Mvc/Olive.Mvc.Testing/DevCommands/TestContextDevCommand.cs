using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class TestContextDevCommand : DevCommand
    {
        public override string Name => "test-context";

        public override Task<string> Run()
        {
            PredictableGuidGenerator.Reset(Param("name"));
            return Task.FromResult(string.Empty);
        }
    }
}
