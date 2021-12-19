using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities.Data;
using Olive.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Aws.Lambda.SQSEvents
{
    public abstract class SQSTriggerHandler<TMessage>
    {
        string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        bool IsProduction => EnvironmentName == "Production";
        bool IsUAT => EnvironmentName == "UAT";
        public SQSTriggerHandler()
        {
            var services = new ServiceCollection();

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (IsProduction)
                configurationBuilder.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);
            if (IsUAT)
                configurationBuilder.AddJsonFile("appsettings.UAT.json", optional: true, reloadOnChange: true);

            configurationBuilder.AddInMemoryCollection(GetEnvironmentVariableConfigs());
            var configuration = configurationBuilder.Build();
            services.AddSingleton<IConfiguration>(configuration);


            services.AddLogging(c => c.AddEventBus());
            services.AddDefaultAudit();
            ConfigureDatabase(services,configuration);
            ConfigureServices(services);
            Context.Initialize(services.BuildServiceProvider(), null);

            Context.Current.GetService<IDatabaseProviderConfig>().Configure();

            OnStartup(configuration);

        }

        protected virtual void ConfigureDatabase(IServiceCollection services,IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddDataAccess(x => x.SqlServer());
        }

        protected virtual void ConfigureServices(IServiceCollection services) { }

        protected virtual void OnStartup(IConfiguration configration) { }
      

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
                await ProcessMessageAsync(message, context);
        }

        private Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)=> 
            Process(Newtonsoft.Json.JsonConvert.DeserializeObject<TMessage>(message.Body));

        protected abstract Task Process(TMessage message);

        static Dictionary<string, string> GetEnvironmentVariableConfigs()
        {
            var memoryConfig = new Dictionary<string, string>();
            foreach (string key in System.Environment.GetEnvironmentVariables().Keys)
            {
                const string PREFIX = "CONFIG__";
                if (!key.StartsWith(PREFIX)) continue;
                var value = System.Environment.GetEnvironmentVariable(key);
                System.Console.WriteLine("Loading " + key + " value : " + value);
                var configKey = key.TrimStart(PREFIX).Replace("__", ":");

                if (configKey.StartsWith("DataReplication:"))
                    configKey = configKey.Replace("_", ".");

                memoryConfig.Add(configKey, value);
            }
            return memoryConfig;
        }
    }
}
