using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using System;

namespace Olive.Aws.Ses.AutoFetch.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();

            Mailbox.Watch("arn:aws:s3:::crm-app-geeks-ltd");


            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Mailbox.FetchAll().GetAwaiter().GetResult();
            }
        }

        static void Init()
        {
            var configuration = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration);

            services.AddDataAccess(x => x.SqlServer());
            services.AddDatabase();
            //services.AddDevCommands(configuration, x => x.AddTempDatabase<SqlServerManager, ReferenceData>().AddClearApiCache());
            services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
            services.AddSingleton<ICache, Cache>();
            services.AddSingleton<IDatabase, Database>();
            Olive.Context.Initialize(services);
            Olive.Context.Current.Set(services.BuildServiceProvider());
            var database = Context.Current.GetService<IDatabase>();
            database.ConfigDataAccess();
            database.Configure();
        }
    }
}
