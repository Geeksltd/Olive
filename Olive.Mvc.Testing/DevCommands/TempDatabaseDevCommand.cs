using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    abstract class TempDatabaseDevCommand : IDevCommand
    {
        bool? IsTempDbMode;
        readonly protected IDatabaseServer DatabaseManager;

        public abstract string Name { get; }

        public virtual string Title => null;

        protected TempDatabaseDevCommand(IDatabaseServer databaseManager)
        {
            DatabaseManager = databaseManager;
        }

        public bool IsEnabled()
        {
            if (IsTempDbMode.HasValue) return IsTempDbMode.Value;

            var database = DatabaseManager.GetDatabaseName().ToLowerOrEmpty();

            if (database.IsEmpty() || database.EndsWith(".temp")) IsTempDbMode = true;
            else if (DatabaseManager.GetDataSource() == ":memory:") IsTempDbMode = true;

            return IsTempDbMode ?? false;
        }

        public abstract Task<string> Run();
    }
}