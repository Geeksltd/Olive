namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RequiredWhenAttribute : RequiredAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if the specified property is true then validate.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public RequiredWhenAttribute(string propertyName) =>
            PropertyName = propertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ConditionalValidatorsHelper.ValidateIfTrue(
                base.IsValid, 
                value, 
                validationContext, 
                PropertyName);
        }
    }
}