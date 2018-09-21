using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class TestContextDevCommand : DevCommand
    {
        public override string Name => "test-context";

        public TestContextDevCommand(IHttpContextAccessor contextAccessor) : base(contextAccessor) { }

        public override Task<bool> Run()
        {
            PredictableGuidGenerator.Reset(Param("name"));
            return Task.FromResult(true);
        }
    }
}
