using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class TempDatabase
    {
        static AsyncLock SyncLock = new AsyncLock();
        public enum CreationStatus { NotCreated, Creating, Created, Failed }

        public static CreationStatus Status { get; internal set; }

        internal static WebTestConfig Config;

        public static async Task Create()
        {
            if (!WebTestConfig.IsActive())
            {
                Debug.WriteLine("Creating temp database aborted. Test mode is not active.");
                return;
            }

            try
            {
                new TestDatabaseGenerator().Process(Config);
                try { await (WebTestConfig.ReferenceDataCreator?.Invoke() ?? Task.CompletedTask); }
                catch (Exception ex)
                {
                    throw new Exception("Failed to run the reference data.", ex);
                }
                Status = CreationStatus.Created;
            }
            catch
            {
                Status = CreationStatus.Failed;
                throw;
            }
        }

        internal static async Task Restart()
        {
            Status = CreationStatus.NotCreated;
            await AwaitReadiness();
            DatabaseChangeWatcher.Restart();
        }

        internal static async Task AwaitReadiness()
        {
            using (await SyncLock.Lock())
            {
                switch (Status)
                {
                    case CreationStatus.Created: break;
                    case CreationStatus.Failed:
                    case CreationStatus.NotCreated:
                        Status = CreationStatus.Creating;
                        await Create();
                        break;
                    case CreationStatus.Creating:
                        await Task.Delay(100);
                        await AwaitReadiness();
                        break;
                }
            }
        }
    }
}
