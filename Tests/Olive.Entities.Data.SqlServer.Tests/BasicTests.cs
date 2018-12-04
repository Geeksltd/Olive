using NUnit.Framework;
using Olive.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities.Data.SqlServer.Tests
{
    
     [TestFixture]
    public class BasicTests : EntityDatabaseTestsBase
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
            await Can_save_to_sqlServer();

            var newPerson = await database.FirstOrDefault<Person>(x => x.FirstName == "Paymon");

            newPerson.ShouldNotBeNull();
            newPerson.FirstName.ShouldEqual("Paymon");
            newPerson.LastName.ShouldEqual("Khamooshi");
        }
    }
}
