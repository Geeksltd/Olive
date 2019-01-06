namespace Olive.Entities
{
    public class DynamicValueCriterion : Criterion
    {
        public DynamicValueCriterion(string leftPropertyName, FilterFunction function, string rightPropertyName)
            : base(leftPropertyName, function, rightPropertyName)
        {
        }

        protected override object GetValue(SqlConversionContext context)
        {
            var valueData = GetColumnName(context, Value.ToString());

            if (FilterFunction == FilterFunction.Contains || FilterFunction == FilterFunction.NotContains) return $"\"%\" + {valueData} + \"%\"";
            else if (FilterFunction == FilterFunction.BeginsWith) return $"{valueData} + \"%\"";
            else if (FilterFunction == FilterFunction.EndsWith) return $"\"%\" + {valueData}";

            return valueData;
        }

        protected override bool NeedsParameter(SqlConversionContext context) => false;
    }
}