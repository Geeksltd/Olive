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
    
    /// <summary>Represents an instance of Group entity type.</summary>
    public partial class Group : GuidEntity
    {
        internal List<GroupMember> cachedGroupMembers;
        
        /* -------------------------- Constructor -----------------------*/
        
        /// <summary>Initializes a new instance of the Group class.</summary>
        public Group() => Deleting.Handle(Cascade_Deleting);
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of DateCreated on this Group instance.</summary>
        public DateTime DateCreated { get; set; }
        
        /// <summary>Gets or sets the value of Name on this Group instance.</summary>
        public string Name { get; set; }
        
        /// <summary>Gets the Group members of this Group.</summary>
        [Calculated]
        [XmlIgnore, Newtonsoft.Json.JsonIgnore]
        public IDatabaseQuery<GroupMember> GroupMembers
        {
            get => Database.Of<GroupMember>().Where(g => g.GroupId == ID);
        }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this Group.</summary>
        public override string ToString() => Name;
        
        /// <summary>Returns a clone of this Group.</summary>
        /// <returns>
        /// A new Group object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new Group Clone() => (Group)base.Clone();
        
        /// <summary>
        /// Validates the data for the properties of this Group and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (Name.IsEmpty())
                result.Add("Name cannot be empty.");
            
            if (Name?.Length > 200)
                result.Add("The provided Name is too long. A maximum of 200 characters is acceptable.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
        
        /// <summary>Throws a validation exception if any record depends on this Group which cannot be cascade-deleted.</summary>
        public virtual async Task ValidateCanBeDeleted()
        {
            if (await GroupMembers.Any())
            {
                throw new ValidationException("This Group cannot be deleted because of {0} dependent Group members.",
                await GroupMembers.Count());
            }
        }
        
        /// <summary>Handles the Deleting event of this Group.</summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The CancelEventArgs instance containing the event data.</param>
        async Task Cascade_Deleting()
        {
            await ValidateCanBeDeleted();
        }
    }
}