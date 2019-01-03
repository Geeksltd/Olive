namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RangeWhenVisibleAttribute : RangeAttribute
    {
        readonly string VisibleProperty;

        /// <summary>
        /// Check if it is not visible skip the validation.
        /// </summary>
        /// <param name="visibleProperty">The boolean property`s name which shows that it is visible.</param>
        public RangeWhenVisibleAttribute(double minimum, double maximum, string visibleProperty = null)
            : base(minimum, maximum) => VisibleProperty = visibleProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="visibleProperty">The boolean property`s name which shows the object will be deleted.</param>
        public RangeWhenVisibleAttribute(int minimum, int maximum, string visibleProperty = null)
            : base(minimum, maximum) => VisibleProperty = visibleProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="visibleProperty">The boolean property`s name which shows the object will be deleted.</param>
        public RangeWhenVisibleAttribute(Type type, string minimum, string maximum, string visibleProperty = null)
            : base(type, minimum, maximum) => VisibleProperty = visibleProperty;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ConditionalValidatorsHelper.ValidateIfTrue(
                base.IsValid,
                value,
                validationContext,
                VisibleProperty ?? validationContext.GetVisiblePropertyName());
        }
    }
}
