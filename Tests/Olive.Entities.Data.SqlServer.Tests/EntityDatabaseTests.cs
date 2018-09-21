using NUnit.Framework;
using Olive.Tests;
using System;
using System.Threading.Tasks;

namespace Olive.Entities.Data.SqlServer.Tests
{
    [TestFixture]
    public partial class EntityDatabaseTests : TestsDatabaseBase
    {
        Person person;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            person = new Person
            {
                FirstName = "Paymon",
                LastName = "Khamooshi"
            };

            #region Create database for Person entity

            var server = new SqlServerManager();

            if (server.Exists(TempDatabaseName, DatabaseFilesPath.FullName)) return;

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

            var newPerson = await database.FirstOrDefault<Person>(x => x.FirstName == "Paymon");

            newPerson.FirstName.ShouldEqual("Paymon");
            newPerson.LastName.ShouldEqual("Khamooshi");
        }
    }
}