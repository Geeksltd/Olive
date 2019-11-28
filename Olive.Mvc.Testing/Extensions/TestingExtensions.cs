using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities.Data;
using System;

namespace Olive.Mvc.Testing
{
    public static class TestingExtensions
    {
        public static DevCommandsOptions AddTempDatabase<TDatabaseServer, TReferenceData>(
            this DevCommandsOptions @this)
            where TDatabaseServer : class, IDatabaseServer
            where TReferenceData : class, IReferenceData
        {
            var services = @this.Services;

            services.AddSingleton<ITempDatabase, TempDatabase>();
            services.AddSingleton<IDatabaseServer, TDatabaseServer>();
            services.AddSingleton<IReferenceData, TReferenceData>();

            services.AddSingleton<IDevCommand, DatabaseRestartDevCommand>();
            services.AddSingleton<IDevCommand, DatabaseGetChangesDevCommand>();
            services.AddSingleton<IDevCommand, DatabaseRunChangesDevCommand>();

            services.AddSingleton<IDevCommand, DatabaseProfileStartDevCommand>();
            services.AddSingleton<IDevCommand, DatabaseProfileSnapshotDevCommand>();
            services.AddSingleton<IDevCommand, DatabaseProfileStopDevCommand>();

            services.AddSingleton<IDevCommand, CsvImportDataDevCommand>();

            return @this;
        }

        public static IServiceCollection AddDevCommands(
            this IServiceCollection @this, Action<DevCommandsOptions> options = null)
        {
            Entities.GuidEntity.NewIdGenerator = PredictableGuidGenerator.Generate;

            @this.AddSingleton<IDevCommand, TestContextDevCommand>();
            @this.AddSingleton<IDevCommand, InjectTimeDevCommand>();
            @this.AddSingleton<IDevCommand, DatabaseClearCacheDevCommand>();

            options?.Invoke(new DevCommandsOptions(@this));

            return @this;
        }
    }
}