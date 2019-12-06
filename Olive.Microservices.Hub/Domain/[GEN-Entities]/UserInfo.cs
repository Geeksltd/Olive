namespace PeopleService
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
    
    /// <summary>Represents an instance of User info entity type.</summary>
    [SoftDelete]
    [Schema("PeopleService")]
    [EscapeGCop("Auto generated code.")]
    public partial class UserInfo : GuidEntity
    {
        /* -------------------------- Constructor -----------------------*/
        
        /// <summary>Initializes a new instance of the UserInfo class.</summary>
        public UserInfo() => Deleting.Handle(Cascade_Deleting);
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of DisplayName on this User info instance.</summary>
        [System.ComponentModel.DisplayName("DisplayName")]
        public string DisplayName { get; set; }
        
        /// <summary>Gets or sets the value of Email on this User info instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Email { get; set; }
        
        /// <summary>Gets or sets the value of ImageUrl on this User info instance.</summary>
        [System.ComponentModel.DisplayName("ImageUrl")]
        public string ImageUrl { get; set; }
        
        /// <summary>Gets or sets a value indicating whether this User info instance is IsActive.</summary>
        [System.ComponentModel.DisplayName("IsActive")]
        public bool IsActive { get; set; }
        
        /// <summary>Gets or sets the value of Roles on this User info instance.</summary>
        public string Roles { get; set; }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this User info.</summary>
        public override string ToString() => DisplayName.OrEmpty();
        
        /// <summary>Returns a clone of this User info.</summary>
        /// <returns>
        /// A new User info object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new UserInfo Clone() => (UserInfo)base.Clone();
        
        /// <summary>
        /// Validates the data for the properties of this User info and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (Email?.Length > 100)
                result.Add("The provided Email is too long. A maximum of 100 characters is acceptable.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Throws a validation exception if any record depends on this User info which cannot be cascade-deleted.<para/>
        /// </summary>
        public virtual async Task ValidateCanBeDeleted()
        {
            var dependantUsers = await Database.Count<Domain.User>(u => u.InfoId == ID);
            
            if (dependantUsers > 0)
            {
                throw new ValidationException("This User info cannot be deleted because of {0} dependent User record(s).", dependantUsers);
            }
        }
        
        /// <summary>Handles the Deleting event of this User info.</summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The CancelEventArgs instance containing the event data.</param>
        async Task Cascade_Deleting()
        {
            await ValidateCanBeDeleted();
        }
    }
}