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
    
    /// <summary>Represents an instance of Permission entity type.</summary>
    [EscapeGCop("Auto generated code.")]
    public partial class Permission : GuidEntity
    {
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of Name on this Permission instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Name { get; set; }
        
        /* -------------------------- Methods ----------------------------*/
        /// <summary>
        /// Find and returns an instance of Permission from the database by its Name.<para/>
        ///                               If no matching Permission is found, it returns Null.<para/>
        /// </summary>
        /// <param name="name">The Name of the requested Permission.</param>
        /// <returns>
        /// The Permission instance with the specified Name or null if there is no Permission with that Name in the database.<para/>
        /// </returns>
        public static Task<Permission> FindByName(string name)
        {
            return Database.FirstOrDefault<Permission>(p => p.Name == name);
        }
        
        /// <summary>Returns a textual representation of this Permission.</summary>
        public override string ToString() => Name;
        
        /// <summary>Returns a clone of this Permission.</summary>
        /// <returns>
        /// A new Permission object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new Permission Clone() => (Permission)base.Clone();
        
        /// <summary>
        /// Validates the data for the properties of this Permission and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override async Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (Name.IsEmpty())
                result.Add("Name cannot be empty.");
            
            if (Name?.Length > 200)
                result.Add("The provided Name is too long. A maximum of 200 characters is acceptable.");
            
            // Ensure uniqueness of Name.
            
            if (await Database.Any<Permission>(p => p.Name == Name && p != this))
                result.Add("Name must be unique. There is an existing Permission record with the provided Name.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
        }
    }
}