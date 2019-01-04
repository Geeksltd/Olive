namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RequiredUnlessAttribute : RequiredAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if the specified property is true then skip the validation.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be ckeck to skip the validation.</param>
        public RequiredUnlessAttribute(string propertyName) =>
            PropertyName = propertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ConditionalValidatorsHelper.ValidateIfFalse(base.IsValid, value, validationContext, PropertyName);
        }
    }
}