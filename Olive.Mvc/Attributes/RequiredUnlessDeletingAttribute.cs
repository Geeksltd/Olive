namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RequiredUnlessDeletingAttribute : RequiredAttribute
    {
        string DeletingProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="deletingProperty">The boolean property`s name which shows the object will be deleted.</param>
        public RequiredUnlessDeletingAttribute(string deletingProperty = "MustBeDeleted") =>
            DeletingProperty = deletingProperty;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) =>
            UnlessDeletingAttributeChecker.IsValid(base.IsValid, value, validationContext, DeletingProperty);
    }

    public class StringLengthUnlessDeletingAttribute : StringLengthAttribute
    {
        string DeletingProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="deletingProperty">The boolean property`s name which shows the object will be deleted.</param>
        public StringLengthUnlessDeletingAttribute(int maximumLength, string deletingProperty = "MustBeDeleted") : base(maximumLength) =>
            DeletingProperty = deletingProperty;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) =>
            UnlessDeletingAttributeChecker.IsValid(base.IsValid, value, validationContext, DeletingProperty);
    }

    internal static class UnlessDeletingAttributeChecker
    {
        public static ValidationResult IsValid(
            Func<object, ValidationContext, ValidationResult> func,
            object value,
            ValidationContext validationContext,
            string deletingProperty
            )
        {
            var property = validationContext.ObjectType.GetProperty(deletingProperty);

            if ((bool)property.GetValue(validationContext.ObjectInstance))
                return ValidationResult.Success;

            return func(value, validationContext);
        }
    }
}