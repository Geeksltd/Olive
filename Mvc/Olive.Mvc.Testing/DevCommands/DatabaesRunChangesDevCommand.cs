using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseRunChangesDevCommand : TempDatabaseDevCommand
    {
        public DatabaseRunChangesDevCommand(IDatabaseServer databaseManager) : base(databaseManager) { }

        public override string Name => "db-run-changes";

        public override async Task<string> Run()
        {
            await DatabaseChangeWatcher.RunChanges();
            return null;
        }
    }
}