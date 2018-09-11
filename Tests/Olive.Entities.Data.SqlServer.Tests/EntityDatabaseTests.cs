using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Olive.Tests;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data.SqlServer.Tests
{
    [TestFixture]
    public partial class EntityDatabaseTests
    {
        Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                ID = "A67A0350-AC6C-4568-8CAA-F1C15802D599".ToLower().To<Guid>(),
                FirstName = "Paymon",
                LastName = "Khamooshi"
            };

            #region initialize Context

            database = new Database();

            var services = new ServiceCollection();

            services.AddSingleton<IDatabase>(database);

            Context.Initialize(services);

            Context.Current.Configure(services.BuildServiceProvider());
            #endregion

            #region Create database for Person

            TempDatabaseName = DatabaseManager.GetDatabaseName();

            var createScript = GetCreateDbFiles().Select(f => File.ReadAllText(f.FullName)).ToLinesString();

            var hash = createScript.ToSimplifiedSHA1Hash().Replace("/", "-").Replace("\\", "-");

            DatabaseFilesPath = DatabaseStoragePath.GetOrCreateSubDirectory(TempDatabaseName).GetOrCreateSubDirectory(hash);

            var server = new SqlServerManager();

            if (server.Exists(TempDatabaseName, DatabaseFilesPath.FullName))
                return;

            server.ClearConnectionPool();
            server.Delete(TempDatabaseName);

            foreach (var file in GetExecutableCreateDbScripts(TempDatabaseName))
            {
                try { server.Execute(file.Value, TempDatabaseName); }
                catch (Exception ex)
                { throw new Exception("Could not execute sql file '" + file.Key.FullName + "'", ex); }
            }

            server.ClearConnectionPool();

            #endregion
        }

        [Test]
        public async Task Can_save_to_sqlServer()
        {
            database.ShouldNotBeNull();

            await database.Save(person);
        }

        [Test]
        public async Task Can_get_from_sqlServer()
        {
            database.ShouldNotBeNull();

            var newPerson = await database.Get<Person>("A67A0350-AC6C-4568-8CAA-F1C15802D599".To<Guid>());

            newPerson.FirstName.ShouldEqual("Paymon");
            newPerson.LastName.ShouldEqual("Khamooshi");
        }

    }
}
