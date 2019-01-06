namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal static class ConditionalValidatorsHelper
    {
        public static ValidationResult ValidateIfFalse(
            Func<object, ValidationContext, ValidationResult> isValid,
            object value,
            ValidationContext validationContext,
            string propertyName)
        {
            return Validate(isValid, value, validationContext, propertyName, expectedValue: false);
        }

        public static ValidationResult ValidateIfTrue(
            Func<object, ValidationContext, ValidationResult> isValid,
            object value,
            ValidationContext validationContext,
            string propertyName)
        {
            return Validate(isValid, value, validationContext, propertyName, expectedValue: true);
        }

        static ValidationResult Validate(
            Func<object, ValidationContext, ValidationResult> isValid,
            object value,
            ValidationContext validationContext,
            string propertyName, 
            bool expectedValue)
        {
            var property = validationContext.ObjectType.GetProperty(propertyName);

            if ((bool)property.GetValue(validationContext.ObjectInstance) != expectedValue)
                return ValidationResult.Success;

            return isValid(value, validationContext);
        }
    }
}
