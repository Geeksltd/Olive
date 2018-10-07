using NUnit.Framework;
using Olive.Tests;

namespace Olive.Entities.Tests
{
    [TestFixture]
    public partial class EntityTests
    {
        Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                FirstName = "Paymon",
                LastName = "Khamooshi"
            };
        }

        [Test]
        public void Can_Clone_Entity()
        {
            var newPerson = person.Clone() as Person;

            newPerson.ShouldNotBeNull();
            newPerson.FirstName.ShouldEqual("Paymon");
            newPerson.LastName.ShouldEqual("Khamooshi");
        }
    }
}
