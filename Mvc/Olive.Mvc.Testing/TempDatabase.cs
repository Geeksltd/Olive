using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class TempDatabase
    {
        internal static bool IsDatabaseBeingCreated;
        static WebTestConfig Config;

        public static async Task Create(WebTestConfig config)
        {
            if (!WebTestConfig.IsActive())
            {
                Debug.WriteLine("Creating temp database aborted. Test mode is not active.");
                return;
            }

            IsDatabaseBeingCreated = true;

            try
            { new TestDatabaseGenerator().Process(Config = config); }
            finally { IsDatabaseBeingCreated = false; }

            // A new database is created. Add the reference data
            try
            {
                await (WebTestConfig.ReferenceDataCreator?.Invoke() ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to run the reference data.", ex);
            }
        }

        public static async Task Start()
        {
            await Create(Config);
            DatabaseChangeWatcher.Restart();
        }

        internal static void AwaitReadiness()
        {
            while (IsDatabaseBeingCreated)
                Thread.Sleep(100); // Wait until it's done.
        }
    }
}
