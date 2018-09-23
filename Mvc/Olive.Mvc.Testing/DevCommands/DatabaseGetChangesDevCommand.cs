using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseGetChangesDevCommand : TempDatabaseDevCommand
    {
        public DatabaseGetChangesDevCommand(IDatabaseServer databaseManager) : base(databaseManager) { }

        public override string Name => "db-get-changes";

        public override async Task<string> Run() => DatabaseChangeWatcher.DispatchChanges();
    }
}
