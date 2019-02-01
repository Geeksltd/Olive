using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TimeAttribute : ValidationAttribute, IClientModelValidator
    {
        public override bool IsValid(object value)
        {
            if (value != null)
                return Regex.IsMatch(value.ToStringOrEmpty(), @"^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$");

            return true;
        }

        public void AddValidation(ClientModelValidationContext context) =>
            MergeAttribute(context.Attributes, "data-val-time", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));

        static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return false;

            attributes.Add(key, value);
            return true;
        }
    }
}