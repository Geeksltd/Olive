using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olive;
using Olive.Audit;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Threading.Tasks;

namespace Olive.Aws.Ses.AutoFetch.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();

            Task.Factory.RunSync(() => Mailbox.Watch("crm-app-geeks-ltd"));


            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("Downloading ...");
                Task.Factory.RunSync(() => Mailbox.FetchAll());
            }
        }

        static void Init()
        {
            var configuration = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();
            var services = new ServiceCollection()
                .AddLogging(c => c.AddConsole())
                .AddSingleton<IConfiguration>(configuration);

            services.AddDataAccess(x => x.SqlServer());
            services.AddDatabase();
            services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
            services.AddSingleton<ICache, Cache>();
            services.AddSingleton<IAudit, Olive.Audit.DefaultAudit>();
            services.AddSingleton<IDatabase, Database>();
            Context.Initialize(services);
            Context.Current.Set(services.BuildServiceProvider());
            var database = Context.Current.GetService<IDatabase>();
            database.ConfigDataAccess();
            database.Configure();
        }
    }
}
