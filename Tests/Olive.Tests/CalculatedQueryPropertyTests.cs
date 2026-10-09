using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Entities.Data;
using System;

namespace Olive.Tests
{
    [TestFixture]
    public class CalculatedQueryPropertyTests
    {
        public class Related : GuidEntity
        {
            public string Name { get; set; }

            [Calculated]
            public string CalculatedName => Name;
        }

        public class Sample : GuidEntity
        {
            public string Name { get; set; }
            public Related Related { get; set; }

            [Calculated]
            public string CalculatedName => Name;

            [Calculated]
            public Related CalculatedRelated => Related;
        }

        IDatabase database;

        [SetUp]
        public void SetUp()
        {
            var provider = new Mock<IDataProvider>();
            var config = new Mock<IDatabaseProviderConfig>();
            config.Setup(x => x.GetProvider(It.IsAny<Type>())).Returns(provider.Object);
            config.Setup(x => x.Configuration).Returns(new DatabaseConfig());

            database = new Database(null, config.Object, new Cache(new InMemoryCacheProvider(), config.Object));
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton(database);
            var serviceProvider = services.BuildServiceProvider();
            Context.Initialize(serviceProvider, () => serviceProvider);
        }

        [Test]
        public void Include_rejects_calculated_associations()
        {
            AssertCalculated(() => database.Of<Sample>().Include(x => x.CalculatedRelated), "Include");
            AssertCalculated(() => database.Of<Sample>().Include("CalculatedRelated"), "Include");
        }

        [Test]
        public void Include_rejects_nested_calculated_properties()
        {
            AssertCalculated(() => database.Of<Sample>().Include("Related.CalculatedName"), "Include");
        }

        [Test]
        public void Where_rejects_calculated_properties_in_different_expressions()
        {
            AssertCalculated(() => database.Of<Sample>().Where(x => x.CalculatedName == "test"), "Where");
            AssertCalculated(() => database.Of<Sample>().Where(x => x.CalculatedName.Contains("test")), "Where");
            AssertCalculated(() => database.Of<Sample>().Where(x => x.Name == "test" || x.CalculatedName == "test"), "Where");
            AssertCalculated(() => database.Of<Sample>().Where(x => x.Related.CalculatedName == "test"), "Where");
            AssertCalculated(() => database.Of<Sample>().Where(new Criterion("CalculatedName", "test")), "Where");
            AssertCalculated(() => database.Of<Sample>().Where(new Criterion("Related.CalculatedName", "test")), "Where");
        }

        [Test]
        public void OrderBy_and_ThenBy_reject_calculated_properties()
        {
            AssertCalculated(() => database.Of<Sample>().OrderBy(x => x.CalculatedName), "OrderBy");
            AssertCalculated(() => database.Of<Sample>().OrderBy("CalculatedName"), "OrderBy");
            AssertCalculated(() => database.Of<Sample>().OrderByDescending(x => x.CalculatedName), "OrderBy");
            AssertCalculated(() => database.Of<Sample>().OrderBy(x => x.Name).ThenBy(x => x.CalculatedName), "OrderBy");
        }

        [Test]
        public void Stored_properties_are_allowed()
        {
            Assert.DoesNotThrow(() => database.Of<Sample>().Where(x => x.Name == "test"));
            Assert.DoesNotThrow(() => database.Of<Sample>().OrderBy(x => x.Name));
            Assert.DoesNotThrow(() => database.Of<Sample>().Include(x => x.Related));
        }

        static void AssertCalculated(TestDelegate action, string operation)
        {
            var error = Assert.Throws<NotSupportedException>(action);
            Assert.That(error.Message, Does.Contain("[Calculated]"));
            Assert.That(error.Message, Does.Contain(operation));
        }
    }
}
