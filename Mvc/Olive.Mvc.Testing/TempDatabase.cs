using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Olive.Entities.Data;

namespace Olive.Mvc.Testing
{
    class TempDatabase
    {
        internal static bool IsDatabaseBeingCreated;
        internal static bool? TempDatabaseInitiated;

        public static async Task Create(bool enforceRestart, bool mustRenew)
        {
            if (!WebTestConfig.IsActive()) return;

            IsDatabaseBeingCreated = true;
            var createdNew = false;

            try
            {
                SqlConnection.ClearAllPools();
                if (enforceRestart) TempDatabaseInitiated = null;
                if (TempDatabaseInitiated.HasValue) return;

                var generator = new TestDatabaseGenerator(isTempDatabaseOptional: true, mustRenew: mustRenew);
                TempDatabaseInitiated = generator.Process();
                createdNew = generator.CreatedNewDatabase;

                await Database.Instance.Refresh();
                SqlConnection.ClearAllPools();
            }
            finally { IsDatabaseBeingCreated = false; }

            if (createdNew)
            {
                // A new database is created. Add the reference data
                await (WebTestConfig.ReferenceDataCreator?.Invoke() ?? Task.CompletedTask);
            }
        }

        public static async Task Start()
        {
            await Create(enforceRestart: true, mustRenew: true);
            DatabaseChangeWatcher.Restart();
        }

        internal static void AwaitReadiness()
        {
            while (IsDatabaseBeingCreated)
                Thread.Sleep(100); // Wait until it's done.
        }
    }
}
