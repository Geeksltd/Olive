using System;
using System.Xml.Serialization;

namespace Olive.Entities.Data.SqlServer.Tests
{
    /// <summary>
    /// This is a mock class for test purpose only
    /// </summary>
    public class Person : GuidEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime Birthdate { get; set; }

        [Calculated]
        [XmlIgnore, Newtonsoft.Json.JsonIgnore]
        public IDatabaseQuery<GroupMember> GroupMembers
        {
            get => Database.Of<GroupMember>().Where(g => g.GroupId == ID);
        }
    }
}
