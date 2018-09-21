using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaesRunChangesDevCommand : TempDatabaseDevCommand
    {
        public DatabaesRunChangesDevCommand(IDatabaseServer databaseManager) : base(databaseManager) { }

        public override string Name => "db-run-changes";

        public override async Task<bool> Run()
        {
            await DatabaseChangeWatcher.RunChanges();
            return true;
        }
    }
}