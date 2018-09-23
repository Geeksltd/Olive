using Olive.Entities.Data;
using Olive.Export;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseProfileDevCommand : DevCommand
    {
        public override string Name => "db-profile";

        public override string Title => "Snapshot Sql Profile";

        public override async Task<string> Run()
        {
            var file = await DataAccessProfiler.GenerateReport(Param("Mode") == "Snapshot").ToCsvFile();
            return "Report generated: " + file.FullName;
        }
    }
}
