namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RangeUnlessDeletingAttribute : RangeAttribute
    {
        readonly string DeletingProperty;

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
            return ConditionalValidatorsHelper.ValidateIfFalse(base.IsValid, value, validationContext, DeletingProperty);
        }
    }
}
