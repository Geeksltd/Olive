namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RangeUnlessAttribute : RangeAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if the specified property is true then skip the validation.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be ckeck to skip the validation.</param>
        public RangeUnlessAttribute(string propertyName, double minimum, double maximum)
            : base(minimum, maximum) => PropertyName = propertyName;

        /// <summary>
        /// Check if the specified property is true then skip the validation.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be ckeck to skip the validation.</param>
        public RangeUnlessAttribute(string propertyName, int minimum, int maximum)
            : base(minimum, maximum) => PropertyName = propertyName;

        /// <summary>
        /// Check if the specified property is true then skip the validation.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be ckeck to skip the validation.</param>
        public RangeUnlessAttribute(string propertyName, Type type, string minimum, string maximum)
            : base(type, minimum, maximum) => PropertyName = propertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ConditionalValidatorsHelper.ValidateIfFalse(base.IsValid, value, validationContext, PropertyName);
        }
    }
}
