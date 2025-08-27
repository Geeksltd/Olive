using Newtonsoft.Json;
using Olive.Entities.Data;
using Olive.Entities.Replication;
using System;
using System.IO;
using System.Linq;
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
                    await CreateRefenceData();
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

        public Task Seed()
        {
            return CreateRefenceData();
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

        async Task CreateRefenceData()
        {
            try
            {
                await ReferenceData.Create();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to run the reference data.", ex);
            }

            try
            {
                await GenerateEndpointsDataFiles();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate endpoints data files.", ex);
            }

            try
            {
                await ReadEndpointsDataFiles();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate endpoints data files.", ex);
            }
        }

        async Task GenerateEndpointsDataFiles()
        {
            var exposedEndpoints = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsA<SourceEndpoint>() && !t.IsAbstract)
                .Select(e => (SourceEndpoint)Activator.CreateInstance(e))
                .ToArray();

            if (exposedEndpoints.None()) return;

            var rootDataDirectory = AppDomain.CurrentDomain.WebsiteRoot().Parent.GetSubDirectory("ReferenceData");
            if (!rootDataDirectory.Exists) rootDataDirectory.Create();

            await exposedEndpoints.Do(async endpoint =>
            {
                endpoint.Publish(false);

                var dataEndpointDirectory = rootDataDirectory.GetSubDirectory(endpoint.GetType().Name);
                if (!dataEndpointDirectory.Exists) dataEndpointDirectory.Create();

                var allTypeMessages = endpoint.GetUploadMessages();

                foreach (var (typeFullName, perTypeMessages) in allTypeMessages)
                {
                    var typeName = typeFullName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
                    var dataDirectory = dataEndpointDirectory.GetSubDirectory(typeName);
                    if (!dataDirectory.Exists) dataDirectory.Create();

                    dataDirectory.GetFiles().Do(f => f.Delete(true));

                    await foreach (var page in perTypeMessages)
                    {
                        foreach (var entity in page)
                        {
                            var message = JsonConvert.SerializeObject(entity);
                            File.WriteAllText(Path.Combine(dataDirectory.FullName, ((ReplicateDataMessage)entity).Entity.ToIOSafeHash() + ".json"), message);
                        }
                    }
                }
            });
        }

        async Task ReadEndpointsDataFiles()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var domainAssembly = assemblies.FirstOrDefault(a => a.GetName().Name.ToLower() == "domain");
            var destinationEndpoints = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsA<DestinationEndpoint>() && !t.IsAbstract)
                .Select(e => (DestinationEndpoint)Activator.CreateInstance(e, domainAssembly))
                .ToArray();

            foreach (var endpoint in destinationEndpoints)
            {
                var assembly = endpoint.GetType().Assembly;

                var jsonFileNames = assembly
                    .GetManifestResourceNames()
                    .Where(name => name.Contains("ReferenceData", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                if (!jsonFileNames.Any()) continue;

                foreach (var jsonFileName in jsonFileNames)
                {
                    using var stream = assembly.GetManifestResourceStream(jsonFileName)
                               ?? throw new InvalidOperationException($"Resource not found: {jsonFileName}");
                    using var reader = new StreamReader(stream);
                    var message = reader.ReadToEnd();
                    await endpoint.Handle(message);
                }
            }
        }
    }
}