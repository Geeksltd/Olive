namespace Olive.Entities.Data.SqlServer.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    
    /// <summary>Represents an instance of Group member entity type.</summary>
    public partial class GroupMember : GuidEntity
    {
        CachedReference<Group> cachedGroup = new CachedReference<Group>();
        Guid? previousGroupId;
        
        CachedReference<Person> cachedPerson = new CachedReference<Person>();
        
        /* -------------------------- Constructor -----------------------*/
        
        /// <summary>Initializes a new instance of the GroupMember class.</summary>
        public GroupMember()
        {
            DateRegistered = LocalTime.Now;
            Loaded.Handle(() => previousGroupId = GroupId);
        }
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of DateRegistered on this Group member instance.</summary>
        public DateTime DateRegistered { get; set; }
        
        /// <summary>Gets or sets the ID of the associated Group.</summary>
        public Guid? GroupId { get; set; }
        
        /// <summary>Gets or sets the value of Group on this Group member instance.</summary>
        public Group Group
        {
            get => cachedGroup.Get(GroupId);
            set => GroupId = value?.ID;
        }
        
        /// <summary>Gets or sets the ID of the associated Person.</summary>
        public Guid? PersonId { get; set; }
        
        /// <summary>Gets or sets the value of Person on this Group member instance.</summary>
        public Person Person
        {
            get => cachedPerson.Get(PersonId);
            set => PersonId = value?.ID;
        }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this Group member.</summary>
        public override string ToString() => $"Group member ({ID})";
        
        /// <summary>Returns a clone of this Group member.</summary>
        /// <returns>
        /// A new Group member object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new GroupMember Clone() => (GroupMember)base.Clone();
        
        public override void InvalidateCachedReferences()
        {
            base.InvalidateCachedReferences();
            
            new [] { GroupId, previousGroupId }.ExceptNull().Distinct()
                .Select(id => Database.Cache.Get<Group>(id)).ExceptNull().Do(x => x.cachedGroupMembers = null);
        }
    }
}