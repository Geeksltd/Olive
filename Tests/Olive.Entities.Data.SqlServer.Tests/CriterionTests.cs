using NUnit.Framework;
using Olive.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities.Data.SqlServer.Tests
{
    public class CriterionTests : EntityDatabaseTestsBase
    {
        const int _30 = 30;
        const int _2000 = 2000;
        const int _10 = 10;

        [Test]
        public async Task Check_FristName_Against_Static_Value()
        {
            database.ShouldNotBeNull();
            await AddPerson("Hamid", "Mayeli", "hamid@mayeli.net", LocalTime.Now.AddYears(-_30));
            await AddPerson("Mohammad", "Alipour", "mohammad@yahoo.com", LocalTime.Now.AddYears(-_30).AddMonths(_30));

            var count = await database.Count<Person>(x => x.FirstName == "Hamid");
            count = count.ShouldEqual(1);
        }

        [Test]
        public async Task Check_Email_Is_Null()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var people = await database.GetList<Person>(x => x.Email.IsEmpty());

            foreach (var person in people)
                person.Email.ShouldBeNull();
        }

        [Test]
        public async Task Check_If_Is_Yahoo_Email()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var count = await database.Count<Person>(x => x.Email.EndsWith("@yahoo.com"));

            count = count.ShouldEqual(10);
        }

        [Test]
        public async Task Check_If_First_And_Last_Name_Are_Equal()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var value = await database.Any<Person>(x => x.LastName == x.FirstName);

            value = value.ShouldEqual(true);
        }

        [Test]
        public async Task Check_If_Email_Contains_Firstname()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var count = await database.Count<Person>(x => x.Email.Contains(x.FirstName));

            count = count.ShouldEqual(10 + 1);
        }

        [Test]
        public async Task Get_Person_Who_Is_Not_In_Groups()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var groupMembers = database.Of<GroupMember>();

            var count = await database.Of<Person>().WhereNotIn(groupMembers, x => x.Person).Count();

            count = count.ShouldEqual(1);
        }


        [Test]
        public async Task Get_Member_Without_Email()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var people = await database.Of<GroupMember>().Where(x => x.Person.Email.IsEmpty()).GetList().Select(x => x.Person);

            foreach (var person in people)
                person.Email.ShouldBeNull();
        }

        [Test]
        public async Task Get_Groups_Where_Any_Member_First_And_Last_Name_Are_Equal()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var groups = await database.Of<GroupMember>().Where(x => x.Group.Name == x.Person.FirstName).GetList().Select(x => x.Group);
        }

        //        select root.* from
        //GroupMembers root
        //left join Groups root_Group on root.Group = root_Group.ID
        //left join Persons root_Person on root.Person = root_Person.ID
        //left join Companies root_Person_Company on rootPerson.Company = root_Person_Company.ID
        //where root_Group.Name = root_Person.Name

        [Test]
        public async Task Get_GroupsMember_Where_Registered_Before_Birth()
        {
            database.ShouldNotBeNull();
            await PopulateTestData();

            var groups = await database.GetList<GroupMember>(x => x.DateRegistered < x.Person.Birthdate).Select(x => x.Group);

            var query = database.Of<Person>().Where(x => x.FirstName == x.LastName);

            //var provider = (AppData.PersonDataProvider)database.GetProvider<Person>();
            //var command = provider.GenerateSelectCommand(query, provider.GetFields());

            //Assert.AreEqual(command, "....");
        }

        async Task PopulateTestData()
        {
            var group1 = await AddGroup("First Group Ever", new DateTime(_2000, 1, 1));
            var group2 = await AddGroup("Hamid", LocalTime.Now.AddYears(-_10));

            for (int index = 1; index <= 10; index++)
            {
                var temp = await AddPerson("Firstname" + index, "Lastname" + index, $"firstname{index}@yahoo.com", LocalTime.Now.AddYears(-index));
                await RegisterInGroup(group1, temp, LocalTime.Now.AddYears(10 - index));
            }

            var javad = await AddPerson("Javad", "Mohammadi", null, new DateTime(_2000, 1, 1));
            await RegisterInGroup(group2, javad, new DateTime(_2000, 10, 10));

            var hamid = await AddPerson("Hamid", "Mayeli", "hamid@mayeli.net", LocalTime.Now.AddYears(-_30));
            await RegisterInGroup(group2, hamid, new DateTime(_2000, 10, 10));

            var ali = await AddPerson("Ali", "Ali", null, LocalTime.Now.AddYears(-_30));
        }

        Task RegisterInGroup(Group group, Person person, DateTime dateTime)
        {
            return database.Save(new GroupMember
            {
                Group = group,
                Person = person,
                DateRegistered = dateTime
            });
        }

        async Task<Group> AddGroup(string name, DateTime dateCreated)
        {
            var result = new Group
            {
                Name = name,
                DateCreated = dateCreated
            };

            await database.Save(result);

            return result;
        }

        async Task<Person> AddPerson(string firstName, string lastName, string email, DateTime birthDate)
        {
            var result = new Person
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Birthdate = birthDate
            };

            await database.Save(result);

            return result;
        }
    }
}
