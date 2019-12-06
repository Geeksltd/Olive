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
    
    /// <summary>Represents an instance of Widget entity type.</summary>
    [TransientEntity]
    [EscapeGCop("Auto generated code.")]
    public partial class Widget : GuidEntity, IComparable<Widget>, ISortable
    {
        /// <summary>The associated Board.</summary>
        Board board;
        
        /* -------------------------- Constructor -----------------------*/
        
        /// <summary>Initializes a new instance of the Widget class.</summary>
        public Widget() => Saving.Handle(Widget_Saving);
        
        /* -------------------------- Properties -------------------------*/
        
        /// <summary>Gets or sets the value of Colour on this Widget instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Colour { get; set; }
        
        /// <summary>Gets or sets the value of Order on this Widget instance.</summary>
        public int Order { get; set; }
        
        /// <summary>Gets or sets the value of Title on this Widget instance.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Title { get; set; }
        
        /// <summary>Gets or sets the value of Board on this Widget instance.</summary>
        public Board Board
        {
            get => board;
            set
            {
                if (board != null && board.Widgets.Contains(this))
                    board.Widgets.Remove(this);
                
                board = value;
                
                if (value != null)
                    value.Widgets.Add(this);
            }
        }
        
        /// <summary>Gets or sets the value of Feature on this Widget instance.</summary>
        public Feature Feature { get; set; }
        
        /// <summary>Gets or sets the value of Settings on this Widget instance.</summary>
        public Feature Settings { get; set; }
        
        /* -------------------------- Methods ----------------------------*/
        
        /// <summary>Returns a textual representation of this Widget.</summary>
        public override string ToString() => Title;
        
        /// <summary>Compares this Widget with another specified Widget instance.</summary>
        /// <param name="other">The other Widget to compare this instance to.</param>
        /// <returns>
        /// An integer value indicating whether this instance precedes, follows, or appears in the same position as the other Widget in sort orders.<para/>
        /// </returns>
        public int CompareTo(Widget other)
        {
            if (other is null)
                return 1;
            else
            {
                return this.Order.CompareTo(other.Order);
            }
        }
        
        /// <summary>Compares this Widget with another object of a compatible type.</summary>
        public override int CompareTo(object other)
        {
            if (other is Widget) return CompareTo(other as Widget);
            else return base.CompareTo(other);
        }
        
        /// <summary>
        /// Validates the data for the properties of this Widget and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();
            
            if (Board == null)
                result.Add("Please provide a value for Board.");
            
            if (Colour.IsEmpty())
                result.Add("Colour cannot be empty.");
            
            if (Colour?.Length > 200)
                result.Add("The provided Colour is too long. A maximum of 200 characters is acceptable.");
            
            if (Feature == null)
                result.Add("Please provide a value for Feature.");
            
            if (Order < 0)
                result.Add("The value of Order must be 0 or more.");
            
            if (Title.IsEmpty())
                result.Add("Title cannot be empty.");
            
            if (Title?.Length > 200)
                result.Add("The provided Title is too long. A maximum of 200 characters is acceptable.");
            
            if (result.Any())
                throw new ValidationException(result.ToLinesString());
            
            return Task.CompletedTask;
        }
        
        /// <summary>Handles the Saving event of the Widget instance.</summary>
        /// <param name="e">The CancelEventArgs instance containing the event data.</param>
        async Task Widget_Saving(System.ComponentModel.CancelEventArgs e)
        {
            if (IsNew && Order == 0)
            {
                // This is a new Widget with unset Order value.
                // So set the Order property so that this Widget goes to the end of the list:
                Order = await Sorter.GetNewOrder(this);
            }
        }
    }
}