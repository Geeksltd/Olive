using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TimeUnlessAttribute : TimeAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if it is a valid time unless the property is true.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be checked to skip the validation.</param>
        public TimeUnlessAttribute(string propertyName) =>
            PropertyName = propertyName;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) =>
            ConditionalValidatorsHelper.ValidateIfFalse(base.IsValid, value, validationContext, PropertyName);
    }
}