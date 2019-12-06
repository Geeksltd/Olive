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
    
    /// <summary>Represents an instance of Authrozied feature info entity type.</summary>
    [TransientEntity]
    [EscapeGCop("Auto generated code.")]
    public partial class AuthroziedFeatureInfo : GuidEntity
    {
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets a value indicating whether this Authrozied feature info instance is IsDisabled.</summary>
        [System.ComponentModel.DisplayName("IsDisabled")]
        public bool IsDisabled { get; set; }
        
        /// <summary>Gets or sets the value of Feature on this Authrozied feature info instance.</summary>
        public Feature Feature { get; set; }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this Authrozied feature info.</summary>
        public override string ToString() => $"Authrozied feature info ({ID})";
    }
}