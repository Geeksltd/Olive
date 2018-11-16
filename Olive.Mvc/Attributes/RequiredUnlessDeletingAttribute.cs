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

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return UnlessDeleting.IsValid(base.IsValid, value, validationContext, DeletingProperty);
        }
    }

    public class RangeUnlessDeletingAttribute : RangeAttribute
    {
        string DeletingProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="deletingProperty">The boolean property`s name which shows the object will be deleted.</param>
        public RangeUnlessDeletingAttribute(double minimum, double maximum, string deletingProperty = "MustBeDeleted")
            : base(minimum, maximum) => DeletingProperty = deletingProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="deletingProperty">The boolean property`s name which shows the object will be deleted.</param>
        public RangeUnlessDeletingAttribute(int minimum, int maximum, string deletingProperty = "MustBeDeleted")
            : base(minimum, maximum) => DeletingProperty = deletingProperty;

        /// <summary>
        /// Check if the object is going to be deleted skip the validation.
        /// </summary>
        /// <param name="deletingProperty">The boolean property`s name which shows the object will be deleted.</param>
        public RangeUnlessDeletingAttribute(Type type, string minimum, string maximum, string deletingProperty = "MustBeDeleted")
            : base(type, minimum, maximum) => DeletingProperty = deletingProperty;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return UnlessDeleting.IsValid(base.IsValid, value, validationContext, DeletingProperty);
        }
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
            UnlessDeleting.IsValid(base.IsValid, value, validationContext, DeletingProperty);
    }

    internal static class UnlessDeleting
    {
        public static ValidationResult IsValid(
            Func<object, ValidationContext, ValidationResult> isValid,
            object value,
            ValidationContext validationContext,
            string deletingProperty)
        {
            var property = validationContext.ObjectType.GetProperty(deletingProperty);

            if ((bool)property.GetValue(validationContext.ObjectInstance))
                return ValidationResult.Success;

            return isValid(value, validationContext);
        }
    }
}