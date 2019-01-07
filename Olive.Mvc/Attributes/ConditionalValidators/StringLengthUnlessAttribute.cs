namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class StringLengthUnlessAttribute : StringLengthAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if the specified property is true then skip the validation.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be ckeck to skip the validation.</param>
        public StringLengthUnlessAttribute(string propertyName, int maximumLength) : base(maximumLength) =>
            PropertyName = propertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) =>
            ConditionalValidatorsHelper.ValidateIfFalse(base.IsValid, value, validationContext, PropertyName);
    }
}
