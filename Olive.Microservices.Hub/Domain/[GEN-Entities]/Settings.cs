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
    
    /// <summary>Represents an instance of Settings entity type.</summary>
    [TableName("Settings")]
    [EscapeGCop("Auto generated code.")]
    public partial class Settings : GuidEntity
    {
        /// <summary>Stores a cache for the Current Settings object.</summary>
        static Settings current;
        
        /* -------------------------- Constructor -----------------------*/
        
        /// <summary>Initializes a new instance of the Settings class.</summary>
        public Settings()
        {
            Saving.HandleWith(ClearCachedInstances);
            Saved.HandleWith(ClearCachedInstances);
            Database.CacheRefreshed.HandleWith(ClearCachedInstances);
        }
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets the Current Settings object.</summary>
        public static Settings Current => current ?? (current = Task.Factory.RunSync(() => Parse("Current")));
        
        /// <summary>Gets or sets the value of Name on this Settings instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Name { get; set; }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>
        /// Returns the Settings instance that is textually represented with a specified string value, or null if no such object is found.<para/>
        /// </summary>
        /// <param name="text">The text representing the Settings to be retrieved from the database.</param>
        /// <returns>The Settings object whose string representation matches the specified text.</returns>
        public static Task<Settings> Parse(string text)
        {
            if (text.IsEmpty())
                throw new ArgumentNullException(nameof(text));
            
            return Database.FirstOrDefault<Settings>(s => s.Name == text);
        }
        
        /// <summary>Returns a textual representation of this Settings.</summary>
        public override string ToString() => Name;
        
        /// <summary>Returns a clone of this Settings.</summary>
        /// <returns>
        /// A new Settings object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new Settings Clone() => (Settings)base.Clone();
        
        /// <summary>
        /// Validates the data for the properties of this Settings and throws a ValidationException if an error is detected.<para/>
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
        
        private static void ClearCachedInstances()
        {
            current = null;
        }
    }
}