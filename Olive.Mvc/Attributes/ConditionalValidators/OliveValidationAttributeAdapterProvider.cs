using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

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

    internal class RangeAttributeAdapter : AttributeAdapterBase<RangeAttribute>
    {
        readonly string _max, _min;

        public RangeAttributeAdapter(RangeAttribute attribute, IStringLocalizer stringLocalizer) : base(attribute, stringLocalizer)
        {
            attribute.IsValid(3);

            _max = Convert.ToString(Attribute.Maximum, CultureInfo.InvariantCulture);
            _min = Convert.ToString(Attribute.Minimum, CultureInfo.InvariantCulture);
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-range", GetErrorMessage(context));
            MergeAttribute(context.Attributes, "data-val-range-max", _max);
            MergeAttribute(context.Attributes, "data-val-range-min", _min);
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName(), Attribute.Minimum, Attribute.Maximum);
        }
    }

    internal class StringLengthAttributeAdapter : AttributeAdapterBase<StringLengthAttribute>
    {
        readonly string _max, _min;

        public StringLengthAttributeAdapter(StringLengthAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        {
            _max = Attribute.MaximumLength.ToString(CultureInfo.InvariantCulture);
            _min = Attribute.MinimumLength.ToString(CultureInfo.InvariantCulture);
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-length", GetErrorMessage(context));

            if (Attribute.MaximumLength != int.MaxValue)
                MergeAttribute(context.Attributes, "data-val-length-max", _max);

            if (Attribute.MinimumLength != 0)
                MergeAttribute(context.Attributes, "data-val-length-min", _min);
        }

        public override string GetErrorMessage(ModelValidationContextBase ctx)
        {
            return GetErrorMessage(ctx.ModelMetadata, ctx.ModelMetadata.GetDisplayName(), Attribute.MaximumLength, Attribute.MinimumLength);
        }
    }
}
