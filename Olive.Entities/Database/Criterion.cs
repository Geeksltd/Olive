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
    public class Criterion : ICriterion
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
            : this(propertyName, function, "(" + ids.Select(x => "'" + x.ToString() + "'").ToString(", ") + ")")
        {
            if (function != FilterFunction.In)
                throw new ArgumentException("List of IDs is only supported with 'FilterFunction.In'.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, FilterFunction function, IEnumerable<string> ids)
            : this(propertyName, function, "(" + ids.Select(x => "'" + x.ToStringOrEmpty().Replace("'", "''") + "'").ToString(", ") + ")")
        {
            if (function != FilterFunction.In)
                throw new ArgumentException("List of IDs is only supported with 'FilterFunction.In'.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criterion"/> class.
        /// </summary>
        public Criterion(string propertyName, FilterFunction function, IEnumerable<int> ids)
            : this(propertyName, function, "(" + ids.ToString(", ") + ")")
        {
            if (function != FilterFunction.In)
                throw new ArgumentException("List of IDs is only supported with 'FilterFunction.In'.");
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

        public static Criterion FromSql(string sqlCondition)
        {
            return new DirectDatabaseCriterion(sqlCondition);
        }

        public static Criterion From<T>(Expression<Func<T, bool>> criterion) where T : IEntity
        {
            if (criterion == null)
                throw new ArgumentNullException(nameof(criterion));

            if (criterion.Body is MethodCallExpression methodCallExpression)
                return From(methodCallExpression);

            if (criterion.Body is BinaryExpression binaryExpression)
                return From(binaryExpression);

            return CriteriaExtractor<T>.CreateCriterion(criterion.Body);
        }

        public static Criterion From(MethodCallExpression expression, bool throwOnError = true)
        {
            Expression valueExpression = null;
            MemberExpression propertyExpression;
            FilterFunction filter;
            string sql = null;

            if (expression.Method == StringContainsMethod)
            {
                propertyExpression = expression.Object as MemberExpression;
                valueExpression = expression.Arguments[0];
                filter = FilterFunction.Contains;
            }
            else if (expression.Method == StringContainsExtensionMethod)
            {
                propertyExpression = expression.Arguments[0] as MemberExpression;
                valueExpression = expression.Arguments[1];
                filter = FilterFunction.Contains;
            }
            else if (expression.Method == StringIsEmptyExtensionMethod)
            {
                propertyExpression = expression.Arguments[0] as MemberExpression;

                sql = "(${{#PROPERTY#}} IS NULL OR ${{#PROPERTY#}} = '')";
                filter = default(FilterFunction);
            }
            else if (expression.Method == StringHasValueExtensionMethod)
            {
                propertyExpression = expression.Arguments[0] as MemberExpression;

                sql = "(${{#PROPERTY#}} IS NOT NULL AND ${{#PROPERTY#}} <> '')";
                filter = default(FilterFunction);
            }
            else if (expression.Method == StringLacksExtensionMethod)
            {
                propertyExpression = expression.Arguments[0] as MemberExpression;
                valueExpression = expression.Arguments[1];
                filter = FilterFunction.NotContains;
            }
            else if (expression.Method == StringStartsWithMethod)
            {
                propertyExpression = expression.Object as MemberExpression;
                valueExpression = expression.Arguments[0];
                filter = FilterFunction.BeginsWith;
            }
            else if (expression.Method == StringEndsWithMethod)
            {
                propertyExpression = expression.Object as MemberExpression;
                valueExpression = expression.Arguments[0];
                filter = FilterFunction.EndsWith;
            }
            else if (expression.Method.Name == "IsAnyOf")
            {
                propertyExpression = expression.Arguments.First() as MemberExpression;
                valueExpression = expression.Arguments[1];
                filter = FilterFunction.In;
            }
            else
            {
                if (!throwOnError) return null;
                throw new ArgumentException("Invalid database criteria. The provided filter expression cannot be evaluated and converted into a SQL condition.");
            }

            if (propertyExpression == null || !(propertyExpression.Member is PropertyInfo))
            {
                if (!throwOnError) return null;
                throw new ArgumentException("Invalid database criteria. The provided filter expression cannot be evaluated and converted into a SQL condition." + expression.ToString() +
                    Environment.NewLine + Environment.NewLine + "Consider using application level filter using the \".Where(...)\" clause.");
            }

            var property = propertyExpression.Member.Name;

            // Middle properties?
            while (propertyExpression.Expression is MemberExpression)
            {
                propertyExpression = propertyExpression.Expression as MemberExpression;
                property = propertyExpression.Member.Name + "." + property;
            }

            if (sql.HasValue())
            {
                if (property.Contains(".")) return null; // Nesting is not supported.
                return new DirectDatabaseCriterion(sql.Replace("#PROPERTY#", property)) { PropertyName = property };
            }
            else
            {
                var value = Expression.Lambda(valueExpression).Compile().DynamicInvoke();

                if (!(value is string) && value is IEnumerable asEnum)
                {
                    Func<object, string> escape = x => $"'{x}'";
                    if (asEnum.GetType().GetEnumerableItemType().IsBasicNumeric())
                        escape = x => x.ToString();

                    if (asEnum.GetType().GetEnumerableItemType().Implements<IEntity>())
                        escape = x => $"'{(x as IEntity).GetId()}'";

                    value = $"({asEnum.Cast<object>().Select(escape).ToString(",")})";
                }

                return new Criterion(property, filter, value);
            }
        }

        static Criterion From(BinaryExpression expression)
        {
            var propertyExpression = expression.Left as MemberExpression;

            if (propertyExpression == null && expression.Left is UnaryExpression)
            {
                propertyExpression = (expression.Left as UnaryExpression).Operand as MemberExpression;
            }

            if (propertyExpression == null || !(propertyExpression.Member is PropertyInfo))
                throw new ArgumentException("Invalid use of Property comparison in condition expression: " + expression.ToString());

            var value = Expression.Lambda(expression.Right).Compile().DynamicInvoke();

            var property = propertyExpression.Member.Name;

            // Middle properties?
            while (propertyExpression.Expression is MemberExpression)
            {
                propertyExpression = propertyExpression.Expression as MemberExpression;
                property = propertyExpression.Member.Name + "." + property;
            }

            return new Criterion(property, GetFilterFunction(expression.NodeType), value);
        }

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