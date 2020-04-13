using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// A basic implementation of a database query criterion.
    /// </summary>
    public partial class Criterion : ICriterion
    {
        const string NULL_ESCAPE = "[#-NULL-VALUE-#]", COLON_ESCAPE = "[#-SEPERATOR-#]";

        static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        static readonly MethodInfo StringContainsExtensionMethod = typeof(OliveExtensions).GetMethod("Contains", new[] { typeof(string), typeof(string), typeof(bool) });
        static readonly MethodInfo StringLacksExtensionMethod = typeof(OliveExtensions).GetMethod("Lacks", new[] { typeof(string), typeof(string), typeof(bool) });
        static readonly MethodInfo StringIsEmptyExtensionMethod = typeof(OliveExtensions).GetMethod("IsEmpty", new[] { typeof(string) });
        static readonly MethodInfo StringHasValueExtensionMethod = typeof(OliveExtensions).GetMethod("HasValue", new[] { typeof(string) });
        static readonly MethodInfo StringStartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        static readonly MethodInfo StringEndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, object value) : this(propertyName, FilterFunction.Is, value) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, FilterFunction function, object value)
        {
            PropertyName = propertyName;
            FilterFunction = function;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, FilterFunction function, IEnumerable<Guid> ids)
            : this(propertyName, function, "(" + ids.Select(x => $"'{x}'").ToString(", ") + ")")
        {
            if (function != FilterFunction.In && function != FilterFunction.NotIn)
                throw new ArgumentException("List of IDs is only supported with 'FilterFunction.In or FilterFunction.NotIn'.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, FilterFunction function, IEnumerable<string> ids)
            : this(propertyName, function, "(" + ids.Select(x => "'" + x.ToStringOrEmpty().Replace("'", "''") + "'").ToString(", ") + ")")
        {
            if (function != FilterFunction.In && function != FilterFunction.NotIn)
                throw new ArgumentException("List of IDs is only supported with 'FilterFunction.In or FilterFunction.NotIn'.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, FilterFunction function, IEnumerable<int> ids)
            : this(propertyName, function, "(" + ids.ToString(", ") + ")")
        {
            if (function != FilterFunction.In && function != FilterFunction.NotIn)
                throw new ArgumentException("List of IDs is only supported with 'FilterFunction.In or FilterFunction.NotIn'.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, string function, object value)
        {
            PropertyName = propertyName;
            FilterFunction = (FilterFunction)Enum.Parse(typeof(FilterFunction), function);
            Value = value;
        }

        /// <summary>
        /// Gets or sets the PropertyName of this Condition.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets or sets the SqlCondition of this Condition.
        /// </summary>
        public string SqlCondition { get; private set; }

        /// <summary>
        /// Gets or sets the Filter Option of this Condition.
        /// </summary>
        public FilterFunction FilterFunction { get; set; }

        /// <summary>
        /// Gets or sets the Value of this Condition.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets a text representation of the value.
        /// </summary>
        string GetSerializedValue()
        {
            if (Value is IEntity)
            {
                return (Value as IEntity).GetId().ToString();
            }
            else if (Value is IEnumerable asEnum && !(Value is string))
            {
                return asEnum.ToString("|");
            }
            else
            {
                return Value?.ToStringOrEmpty();
            }
        }

        /// <summary>
        /// Returns a string that represents this instance.
        /// </summary>
        public override string ToString()
        {
            if (SqlCondition.HasValue()) return SqlCondition;

            var valueText = Value == null ? NULL_ESCAPE : GetSerializedValue().Replace(":", COLON_ESCAPE);

            return string.Join(":", PropertyName, FilterFunction, valueText);
        }

        /// <summary>
        /// Parses the specified condition string.
        /// </summary>
        public static Criterion Parse(string criterionString)
        {
            if (criterionString.IsEmpty())
                throw new ArgumentNullException(nameof(criterionString));

            var parts = criterionString.Split(':');

            var value = parts[2].Replace(COLON_ESCAPE, ":");
            if (value == NULL_ESCAPE) value = null;

            return new Criterion(parts[0], parts[1], value);
        }

        object ICriterion.Value => (Value as IEntity)?.GetId() ?? Value;

        /// <summary>
        /// Gets the filter option for a specified Lambda expression node type.
        /// </summary>
        static FilterFunction GetFilterFunction(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal: return FilterFunction.Is;
                case ExpressionType.NotEqual: return FilterFunction.IsNot;
                case ExpressionType.GreaterThan: return FilterFunction.MoreThan;
                case ExpressionType.GreaterThanOrEqual: return FilterFunction.MoreThanOrEqual;
                case ExpressionType.LessThan: return FilterFunction.LessThan;
                case ExpressionType.LessThanOrEqual: return FilterFunction.LessThanOrEqual;
                default:
                    throw new NotSupportedException("GetFilterFunction() does not support expression of type " + nodeType);
            }
        }
    }
}