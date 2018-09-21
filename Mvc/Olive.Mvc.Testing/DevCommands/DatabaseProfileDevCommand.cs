using Microsoft.AspNetCore.Http;
using Olive.Entities.Data;
using Olive.Export;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseProfileDevCommand : DevCommand
    {
        public DatabaseProfileDevCommand(IHttpContextAccessor contextAccessor) : base(contextAccessor) { }

        public override string Name => "db-profile";

        public override string Title => "Snapshot Sql Profile";

        public override async Task<bool> Run()
        {
            var file = await DataAccessProfiler.GenerateReport(Param("Mode") == "Snapshot").ToCsvFile();
            await Context.Response.EndWith("Report generated: " + file.FullName);
            return true;
        }
    }
}
