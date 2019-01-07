namespace Olive.Mvc
{
    using System.ComponentModel.DataAnnotations;

    public class DataTypeWhenAttribute : DataTypeAttribute
    {
        readonly string PropertyName;

        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public DataTypeWhenAttribute(string propertyName, DataType dataType) : base(dataType)
        {
            PropertyName = propertyName;
        }

        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public DataTypeWhenAttribute(string propertyName, string customDataType) : base(customDataType)
        {
            PropertyName = propertyName;
        }

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
