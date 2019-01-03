namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class StringLengthWhenVisibleAttribute : StringLengthAttribute
    {
        readonly string VisibleProperty;

        /// <summary>
        /// Check if it is not visible skip the validation.
        /// </summary>
        /// <param name="visibleProperty">The boolean property`s name which shows that it is visible.</param>
        public StringLengthWhenVisibleAttribute(int maximumLength, string visibleProperty = null) : base(maximumLength) =>
            VisibleProperty = visibleProperty;

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
