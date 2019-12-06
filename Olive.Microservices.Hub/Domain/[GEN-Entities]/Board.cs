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
    
    /// <summary>Represents an instance of Board entity type.</summary>
    [TransientEntity]
    [EscapeGCop("Auto generated code.")]
    public partial class Board : GuidEntity
    {
        /// <summary>The associated Widgets.</summary>
        private IList<Widget> widgets = new List<Widget>();
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of Name on this Board instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Name { get; set; }
        
        /// <summary>Gets the Widgets of this Board.</summary>
        [XmlIgnore, Newtonsoft.Json.JsonIgnore]
        [InverseOf("Board")]
        public IList<Widget> Widgets
        {
            get => widgets;
        }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this Board.</summary>
        public override string ToString() => Name;
        
        /// <summary>
        /// Validates the data for the properties of this Board and throws a ValidationException if an error is detected.<para/>
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
    }
}