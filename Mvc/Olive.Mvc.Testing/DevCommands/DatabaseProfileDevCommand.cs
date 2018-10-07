using Olive.Entities.Data;
using Olive.Export;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseProfileStartDevCommand : DevCommand
    {
        public override string Name => "db-profile-start";

        public override string Title => "SQL Profiling: Start";

        public override async Task<string> Run()
        {
            DataAccessProfiler.Reset();
            return null;
        }
    }

    class DatabaseProfileStopDevCommand : DevCommand
    {
        public override string Name => "db-profile-stop";

        public override string Title => "SQL Profiling: Stop";

        public override async Task<string> Run()
        {
            var file = await DataAccessProfiler.GenerateReport(snapshot: false).ToCsvFile();
            return "Report generated: " + file.FullName;
        }
    }

    class DatabaseProfileSnapshotDevCommand : DevCommand
    {
        public override string Name => "db-profile-snapshot";

        public override string Title => "SQL Profiling: Take snapshot";

        public override async Task<string> Run()
        {
            var file = await DataAccessProfiler.GenerateReport(snapshot: true).ToCsvFile();
            return "Report generated: " + file.FullName;
        }
    }
}