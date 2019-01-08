namespace Olive.Mvc
{
    using System.ComponentModel.DataAnnotations;

    public class DataTypeUnlessAttribute : DataTypeAttribute
    {
        readonly string PropertyName;

        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public DataTypeUnlessAttribute(string propertyName, DataType dataType) : base(dataType)
        {
            PropertyName = propertyName;
        }

        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public DataTypeUnlessAttribute(string propertyName, string customDataType) : base(customDataType)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ConditionalValidatorsHelper.ValidateIfFalse(
                base.IsValid,
                value,
                validationContext,
                PropertyName);
        }
    }
}
