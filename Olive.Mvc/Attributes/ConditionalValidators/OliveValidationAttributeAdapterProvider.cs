using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.Extensions.Localization;
using Olive;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Olive.Mvc
{
    public class OliveValidationAttributeAdapterProvider :
        IValidationAttributeAdapterProvider
    {
        IValidationAttributeAdapterProvider baseProvider = new ValidationAttributeAdapterProvider();
        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute,
            IStringLocalizer stringLocalizer)
        {
            if (attribute is RequiredUnlessAttribute ||
                attribute is RequiredWhenAttribute)
                return new RequiredAttributeAdapter(attribute as RequiredAttribute, stringLocalizer);


            if (attribute is RangeUnlessAttribute ||
                attribute is RangeWhenAttribute)
                return new RangeAttributeAdapter(attribute as RangeAttribute, stringLocalizer);


            if (attribute is StringLengthUnlessAttribute ||
                attribute is StringLengthWhenAttribute)
                return new StringLengthAttributeAdapter(attribute as StringLengthAttribute, stringLocalizer);

            else return baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
