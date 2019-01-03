namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RequiredWhenVisibleAttribute : RequiredAttribute
    {
        readonly string VisibleProperty;

        /// <summary>
        /// Check if it is not visible skip the validation.
        /// </summary>
        /// <param name="visibleProperty">The boolean property`s name which shows that it is visible.</param>
        public RequiredWhenVisibleAttribute(string visibleProperty = null) =>
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