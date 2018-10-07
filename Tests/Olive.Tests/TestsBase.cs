using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using System.IO;

namespace Olive.Tests
{
    public abstract class TestsBase
    {
        protected MockRepository mocks;
        protected IDatabase database;
        protected string TempDatabaseName;
        protected DirectoryInfo DbDirectory, DatabaseFilesPath;
        protected IServiceCollection services;
        protected IConfigurationBuilder Configuration;

        [SetUp]
        public virtual void SetUp()
        {
            mocks = new MockRepository(MockBehavior.Loose);

            services = new ServiceCollection();
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            services.AddSingleton<IConfiguration>(Configuration.Build());
        }

        [TearDown]
        public virtual void TearDown() => mocks?.VerifyAll();
    }
}
