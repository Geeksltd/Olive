using Olive.Entities.Data;
using System;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    public enum TempDatabaseCreationStatus { NotCreated, Creating, Created, Failed }

    class TempDatabase : ITempDatabase
    {
        static AsyncLock SyncLock = new AsyncLock();

        public TempDatabaseCreationStatus Status { get; internal set; }
        IDatabaseServer DatabaseManager;
        IReferenceData ReferenceData;

        public TempDatabase(IReferenceData referenceData, IDatabaseServer databaseManager)
        {
            ReferenceData = referenceData;
            DatabaseManager = databaseManager;
        }

        public async Task Create(bool dropExisting = false)
        {
            try
            {
                var generator = new TestDatabaseGenerator(DatabaseManager)
                {
                    DropExisting = dropExisting
                };

                if (generator.Process())
                {
                    try { await ReferenceData.Create(); }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to run the reference data.", ex);
                    }
                }

                Status = TempDatabaseCreationStatus.Created;
            }
            catch
            {
                Status = TempDatabaseCreationStatus.Failed;
                throw;
            }
            finally
            {
                DatabaseChangeWatcher.Restart();
            }
        }

        public async Task ReCreate(bool dropExisting = false)
        {
            try
            {
                var generator = new TestDatabaseGenerator(DatabaseManager) { DropExisting = dropExisting };

                generator.Process();
                Status = TempDatabaseCreationStatus.Created;
            }
            catch
            {
                Status = TempDatabaseCreationStatus.Failed;
                throw;
            }
            finally
            {
                DatabaseChangeWatcher.Restart();
            }
        }


        public async Task Restart()
        {
            using (await SyncLock.Lock())
            {
                Status = TempDatabaseCreationStatus.Creating;
                await Create(dropExisting: true);
            }
        }

        public async Task ReCreateDb()
        {
            using (await SyncLock.Lock())
            {
                Status = TempDatabaseCreationStatus.Creating;
                await ReCreate(dropExisting: true);
            }
        }

        public async Task Seed()
        {
            try { await ReferenceData.Create(); }
            catch (Exception ex)
            {
                throw new Exception("Failed to run the reference data.", ex);
            }
        }

        public async Task AwaitReadiness()
        {
            using (await SyncLock.Lock())
            {
                switch (Status)
                {
                    case TempDatabaseCreationStatus.Created: break;
                    case TempDatabaseCreationStatus.Failed:
                    case TempDatabaseCreationStatus.NotCreated:
                        Status = TempDatabaseCreationStatus.Creating;
                        await Create();
                        break;
                    case TempDatabaseCreationStatus.Creating:
                        await Task.Delay(100);
                        await AwaitReadiness();
                        break;
                    default: throw new NotSupportedException(Status + " is not handled");
                }
            }
        }

    }
}