using NUnit.Framework;
using Olive.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities.Data.SqlServer.Tests
{
    partial class EntityDatabaseTests
    {
        const int _30 = 30;

        [Test]
        public async Task Check_FristName_Against_Static_Value()
        {
            database.ShouldNotBeNull();
            await AddPerson("Hamid", "Mayeli", "hamid@mayeli.net", LocalTime.Now.AddYears(-_30));
            await AddPerson("Mohammad", "Alipour", "mohammad@yahoo.com", LocalTime.Now.AddYears(-_30).AddMonths(_30));

            var count = await database.Count<Person>(x => x.FirstName == "Hamid");
            count = count.ShouldEqual(1);
        }

        Task AddPerson(string firstName, string lastName, string email, DateTime birthDate)
        {
            return database.Save(new Person
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Birthdate = birthDate
            });
        }
    }
}
