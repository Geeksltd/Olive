using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TimeWhenAttribute : TimeAttribute
    {
        readonly string PropertyName;

        /// <summary>
        /// Check if it is a valid time.
        /// </summary>
        /// <param name="propertyName">The boolean property`s name which will be checked before validating.</param>
        public TimeWhenAttribute(string propertyName) =>
            PropertyName = propertyName;

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