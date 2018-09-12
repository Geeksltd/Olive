using NUnit.Framework;
using Olive.Tests;
using System;

namespace Olive.Entities.Tests
{
    [TestFixture]
    public partial class EntityTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                ID = "A67A0350-AC6C-4568-8CAA-F1C15802D599".ToLower().To<Guid>(),
                FirstName = "Paymon",
                LastName = "Khamooshi"
            };
        }

        [Test]
        public void Can_Clone_Entity()
        {
            var newPerson = person.Clone() as Person;

            newPerson.ShouldNotBeNull();
            newPerson.ID.ShouldEqual("A67A0350-AC6C-4568-8CAA-F1C15802D599".ToLower().To<Guid>());
            newPerson.FirstName.ShouldEqual("Paymon");
            newPerson.LastName.ShouldEqual("Khamooshi");
        }
    }
}
