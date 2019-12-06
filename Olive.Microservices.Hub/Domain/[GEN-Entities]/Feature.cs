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
    
    /// <summary>Represents an instance of Feature entity type.</summary>
    [TransientEntity]
    [EscapeGCop("Auto generated code.")]
    public partial class Feature : GuidEntity, IComparable<Feature>, IHierarchy, ISortable
    {
        /// <summary>Stores the associated Features for Children property.</summary>
        private IList<Feature> children = new List<Feature>();
        
        /// <summary>Stores the associated Permissions for NotPermissions property.</summary>
        private IList<Permission> notPermissions = new List<Permission>();
        
        /// <summary>Stores the associated Permissions for Permissions property.</summary>
        private IList<Permission> permissions = new List<Permission>();
        
        /* -------------------------- Constructor -----------------------*/
        
        /// <summary>Initializes a new instance of the Feature class.</summary>
        public Feature() => Saving.Handle(Feature_Saving);
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of BadgeOptionalFor on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string BadgeOptionalFor { get; set; }
        
        /// <summary>Gets or sets the value of BadgeUrl on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string BadgeUrl { get; set; }
        
        /// <summary>Gets or sets the value of Description on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Description { get; set; }
        
        /// <summary>Gets or sets the value of Icon on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Icon { get; set; }
        
        /// <summary>Gets or sets the value of ImplementationUrl on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string ImplementationUrl { get; set; }
        
        /// <summary>Gets or sets the value of LoadUrl on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string LoadUrl { get; set; }
        
        /// <summary>Gets or sets the value of Order on this Feature instance.</summary>
        public int Order { get; set; }
        
        /// <summary>Gets or sets the value of PositionOrder on this Feature instance.</summary>
        [System.ComponentModel.DisplayName("Position Order")]
        public double? PositionOrder { get; set; }
        
        /// <summary>Gets or sets the value of Ref on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Ref { get; set; }
        
        /// <summary>Gets or sets a value indicating whether this Feature instance Show on right.</summary>
        public bool ShowOnRight { get; set; }
        
        /// <summary>Gets or sets the value of Title on this Feature instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Title { get; set; }
        
        /// <summary>Gets or sets a value indicating whether this Feature instance Use iframe.</summary>
        public bool UseIframe { get; set; }
        
        /// <summary>Gets or sets the Children of this Feature.</summary>
        public IEnumerable<Feature> Children
        {
            get => children;
            
            set
            {
                children = value?.ToList() ?? new List<Feature>();
            }
        }
        
        /// <summary>Gets the value of GrandParent on this Feature instance.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.ComponentModel.DisplayName("GrandParent")]
        public Feature GrandParent
        {
            get => Parent?.Parent;
        }
        
        /// <summary>Gets or sets the Not permissions of this Feature.</summary>
        public IEnumerable<Permission> NotPermissions
        {
            get => notPermissions;
            
            set
            {
                notPermissions = value?.ToList() ?? new List<Permission>();
            }
        }
        
        /// <summary>Gets or sets the value of Parent on this Feature instance.</summary>
        public Feature Parent { get; set; }
        
        /// <summary>Gets or sets the Permissions of this Feature.</summary>
        public IEnumerable<Permission> Permissions
        {
            get => permissions;
            
            set
            {
                permissions = value?.ToList() ?? new List<Permission>();
            }
        }
        
        /// <summary>Gets or sets the value of Service on this Feature instance.</summary>
        public Service Service { get; set; }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this Feature.</summary>
        public override string ToString() => this.WithAllParents().Select(s=>s.Title).ToString(" > ");
        
        /// <summary>Compares this Feature with another specified Feature instance.</summary>
        /// <param name="other">The other Feature to compare this instance to.</param>
        /// <returns>
        /// An integer value indicating whether this instance precedes, follows, or appears in the same position as the other Feature in sort orders.<para/>
        /// </returns>
        public int CompareTo(Feature other)
        {
            if (other is null)
                return 1;
            else
            {
                return this.Order.CompareTo(other.Order);
            }
        }
        
        /// <summary>Compares this Feature with another object of a compatible type.</summary>
        public override int CompareTo(object other)
        {
            if (other is Feature) return CompareTo(other as Feature);
            else return base.CompareTo(other);
        }
        
        /// <summary>Adds a single Feature  to the Children of this Feature.</summary>
        /// <param name="item">The instance to add to Children of this Feature.</param>
        public virtual void AddToChildren(Feature item)
        {
            if (item != null && !children.Contains(item))
            {
                children.Add(item);
            }
        }
        
        /// <summary>Removes a specified Feature object from the Children of this Feature.</summary>
        /// <param name="item">The instance to remove from Children of this Feature.</param>
        public virtual void RemoveFromChildren(Feature item)
        {
            if (item != null && children.Contains(item))
            {
                children.Remove(item);
            }
        }
        
        /// <summary>Adds a single Permission  to the Not permissions of this Feature.</summary>
        /// <param name="item">The instance to add to Not permissions of this Feature.</param>
        public virtual void AddToNotPermissions(Permission item)
        {
            if (item != null && !notPermissions.Contains(item))
            {
                notPermissions.Add(item);
            }
        }
        
        /// <summary>Removes a specified Permission object from the Not permissions of this Feature.</summary>
        /// <param name="item">The instance to remove from Not permissions of this Feature.</param>
        public virtual void RemoveFromNotPermissions(Permission item)
        {
            if (item != null && notPermissions.Contains(item))
            {
                notPermissions.Remove(item);
            }
        }
        
        /// <summary>Adds a single Permission  to the Permissions of this Feature.</summary>
        /// <param name="item">The instance to add to Permissions of this Feature.</param>
        public virtual void AddToPermissions(Permission item)
        {
            if (item != null && !permissions.Contains(item))
            {
                permissions.Add(item);
            }
        }
        
        /// <summary>Removes a specified Permission object from the Permissions of this Feature.</summary>
        /// <param name="item">The instance to remove from Permissions of this Feature.</param>
        public virtual void RemoveFromPermissions(Permission item)
        {
            if (item != null && permissions.Contains(item))
            {
                permissions.Remove(item);
            }
        }
        
        /// <summary>
        /// Validates the data for the properties of this Feature and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (BadgeOptionalFor?.Length > 200)
                result.Add("The provided Badge optional for is too long. A maximum of 200 characters is acceptable.");
            
            if (BadgeUrl?.Length > 200)
                result.Add("The provided Badge url is too long. A maximum of 200 characters is acceptable.");
            
            if (Description?.Length > 200)
                result.Add("The provided Description is too long. A maximum of 200 characters is acceptable.");
            
            if (Icon?.Length > 200)
                result.Add("The provided Icon is too long. A maximum of 200 characters is acceptable.");
            
            if (ImplementationUrl?.Length > 200)
                result.Add("The provided Implementation url is too long. A maximum of 200 characters is acceptable.");
            
            if (LoadUrl.IsEmpty())
                result.Add("Load url cannot be empty.");
            
            if (LoadUrl?.Length > 200)
                result.Add("The provided Load url is too long. A maximum of 200 characters is acceptable.");
            
            if (Order < 0)
                result.Add("The value of Order must be 0 or more.");
            
            if (PositionOrder < 0)
                result.Add("The value of Position Order must be 0 or more.");
            
            if (Ref?.Length > 200)
                result.Add("The provided Ref is too long. A maximum of 200 characters is acceptable.");
            
            if (Service == null)
                result.Add("Please provide a value for Service.");
            
            if (Title.IsEmpty())
                result.Add("Title cannot be empty.");
            
            if (Title?.Length > 200)
                result.Add("The provided Title is too long. A maximum of 200 characters is acceptable.");
            
            if (this.GetParent() != null)
            {
                if (this.WithAllChildren().Contains(this.GetParent()))
                    result.Add(string.Format("Invalid parent selected for this Feature. Setting {0} as the parent node of {1} will create an infinite loop.", this.GetParent(), this));
            }
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
        
        /// <summary>Handles the Saving event of the Feature instance.</summary>
        /// <param name="e">The CancelEventArgs instance containing the event data.</param>
        async Task Feature_Saving(System.ComponentModel.CancelEventArgs e)
        {
            if (IsNew && Order == 0)
            {
                // This is a new Feature with unset Order value.
                // So set the Order property so that this Feature goes to the end of the list:
                Order = await Sorter.GetNewOrder(this);
            }
        }
    }
}