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

            services.AddTransient<ITempDatabase, TempDatabase>();
            services.AddTransient<IDatabaseServer, TDatabaseServer>();
            services.AddTransient<IReferenceData, TReferenceData>();

            services.AddTransient<IDevCommand, DatabaseRestartDevCommand>();
            services.AddTransient<IDevCommand, DatabaseGetChangesDevCommand>();
            services.AddTransient<IDevCommand, DatabaseRunChangesDevCommand>();
            services.AddTransient<IDevCommand, DatabaseProfileStartDevCommand>();
            services.AddTransient<IDevCommand, DatabaseProfileSnapshotDevCommand>();
            services.AddTransient<IDevCommand, DatabaseProfileStopDevCommand>();
            services.AddTransient<IDevCommand, CsvImportDataDevCommand>();

            return @this;
        }

        public static IServiceCollection AddDevCommands(
            this IServiceCollection @this, Action<DevCommandsOptions> options = null)
        {
            Entities.GuidEntity.NewIdGenerator = PredictableGuidGenerator.Generate;

            @this.AddTransient<IDevCommand, TestContextDevCommand>();
            @this.AddTransient<IDevCommand, InjectTimeDevCommand>();
            @this.AddTransient<IDevCommand, DatabaseClearCacheDevCommand>();

            options?.Invoke(new DevCommandsOptions(@this));

            return @this;
        }
    }
}