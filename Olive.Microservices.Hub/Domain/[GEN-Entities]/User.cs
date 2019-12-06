namespace Domain
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
    
    /// <summary>Represents an instance of User entity type.</summary>
    [EscapeGCop("Auto generated code.")]
    public partial class User : GuidEntity
    {
        CachedReference<PeopleService.UserInfo> cachedInfo = new CachedReference<PeopleService.UserInfo>();
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of Email on this User instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Email { get; set; }
        
        /// <summary>Gets or sets the ID of the associated Info.</summary>
        public Guid? InfoId { get; set; }
        
        /// <summary>Gets or sets the value of Info on this User instance.</summary>
        public PeopleService.UserInfo Info
        {
            get => cachedInfo.GetOrDefault(InfoId);
            set => InfoId = value?.ID;
        }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this User.</summary>
        public override string ToString() => $"User ({ID})";
        
        /// <summary>Returns a clone of this User.</summary>
        /// <returns>
        /// A new User object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new User Clone() => (User)base.Clone();
        
        /// <summary>
        /// Validates the data for the properties of this User and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (Email?.Length > 200)
                result.Add("The provided Email is too long. A maximum of 200 characters is acceptable.");
            
            // Ensure Email matches Email address pattern:
            
            if (Email.HasValue() && !System.Text.RegularExpressions.Regex.IsMatch(Email, "\\s*\\w+([-+.'\\w])*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*\\s*"))
                result.Add("The provided Email is not a valid Email address.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
    }
}