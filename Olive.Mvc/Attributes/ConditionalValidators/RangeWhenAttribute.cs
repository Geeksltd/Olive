namespace Olive.Mvc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RangeWhenAttribute : RangeAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if the specified property is true then validate.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public RangeWhenAttribute(double minimum, double maximum, string propertyName)
            : base(minimum, maximum) => PropertyName = propertyName;

        /// <summary>
        /// Check if the specified property is true then validate.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public RangeWhenAttribute(int minimum, int maximum, string propertyName)
            : base(minimum, maximum) => PropertyName = propertyName;

        /// <summary>
        /// Check if the specified property is true then validate.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public RangeWhenAttribute(Type type, string minimum, string maximum, string propertyName)
            : base(type, minimum, maximum) => PropertyName = propertyName;

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
