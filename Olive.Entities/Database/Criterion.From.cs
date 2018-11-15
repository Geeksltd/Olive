using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive.Entities
{
    partial class Criterion
    {
        public static Criterion FromSql(string sqlCondition)
        {
            return new DirectDatabaseCriterion(sqlCondition);
        }

        public static Criterion From<T>(Expression<Func<T, bool>> criterion) where T : IEntity
        {
            return From(criterion.Body);
        }

        public static Criterion From(Expression expression)
        {
            if (expression is BinaryExpression binary) return From(binary);
            if (expression is UnaryExpression unary) return From(unary);
            if (expression is MemberExpression member) return From(member);
            if (expression is MethodCallExpression method) return From(method, throwOnError: false);
            return null;
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
                var column = propertyExpression.GetDatabaseColumnPath();

                return new BinaryCriterion(new Criterion(column, value: null),
                    BinaryOperator.OR, new Criterion(column, string.Empty));
            }
            else if (expression.Method == StringHasValueExtensionMethod)
            {
                propertyExpression = expression.Arguments[0] as MemberExpression;
                var column = propertyExpression.GetDatabaseColumnPath();

                return new BinaryCriterion(new Criterion(column, FilterFunction.IsNot, value: null),
                   BinaryOperator.AND, new Criterion(column, FilterFunction.IsNot, string.Empty));
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
                propertyExpression = (propertyExpression.Expression as MemberExpression);
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
                        escape = x => $"'{(x as IEntity).GetId().ToString()}'";

                    value = $"({asEnum.Cast<object>().Select(escape).ToString(",")})";
                }

                return new Criterion(property, filter, value);
            }
        }

        public static Criterion From(MemberExpression expression)
        {
            if (expression.NodeType != ExpressionType.MemberAccess) return null;

            if(expression.GetPropertiesInPath().LastOrDefault()?.Name == "HasValue" &&
                expression.Expression is MemberExpression memberExpression)
            {
                var nullableProperty = memberExpression.GetDatabaseColumnPath();
                return new Criterion(nullableProperty, FilterFunction.IsNot, value: null);
            }

            var property = expression.GetDatabaseColumnPath();
            if (property.IsEmpty()) return null;

            if ((expression.Member as PropertyInfo).PropertyType != typeof(bool)) return null;

            // Only one property level is supported:
            return new Criterion(property, FilterFunction.Is, true);
        }

        public static Criterion From(UnaryExpression expression)
        {
            if (expression.NodeType != ExpressionType.Not) return null;

            if (expression.Operand is MemberExpression member)
            {
                var criterion = From(member);
                if (criterion != null)
                    criterion.FilterFunction = criterion.FilterFunction == FilterFunction.Is ? FilterFunction.IsNot : FilterFunction.Is;

                return criterion;
            }

            return null;
        }

        public static Criterion From(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.OrElse || expression.NodeType == ExpressionType.AndAlso)
                return BinaryCriterion.From(expression);

            Criterion create(string property)
            {
                if (property.IsEmpty()) return null;
                return new Criterion(property, expression.NodeType.ToFilterFunction(), expression.Right.GetValue());
            }

            var left = expression.Left;

            if (left is ParameterExpression) return create("ID");

            if (left is MemberExpression memberEx)
                return create(memberEx.GetDatabaseColumnPath());

            if (left is UnaryExpression unary && unary.Operand is MemberExpression member)
                return create(member.GetDatabaseColumnPath());

            return null;
        }
    }
}