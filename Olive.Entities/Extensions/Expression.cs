using Olive.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive
{
    partial class OliveEntitiesExtensions
    {
        /// <summary>
        /// Get the equivalent of the given expression type.
        /// </summary>
        /// <param name="flipped">Pass true to get less instead of more and vice versa.</param>
        public static FilterFunction ToFilterFunction(this ExpressionType type, bool flipped = false)
        {
            if (flipped)
                return type switch
                {
                    ExpressionType.Equal => FilterFunction.Is,
                    ExpressionType.NotEqual => FilterFunction.IsNot,
                    ExpressionType.GreaterThan => FilterFunction.LessThan,
                    ExpressionType.GreaterThanOrEqual => FilterFunction.LessThanOrEqual,
                    ExpressionType.LessThan => FilterFunction.MoreThan,
                    ExpressionType.LessThanOrEqual => FilterFunction.MoreThanOrEqual,
                    _ => throw new NotSupportedException(type + " is still not supported as a FilterFunction."),
                };
            else
                return type switch
                {
                    ExpressionType.Equal => FilterFunction.Is,
                    ExpressionType.NotEqual => FilterFunction.IsNot,
                    ExpressionType.GreaterThan => FilterFunction.MoreThan,
                    ExpressionType.GreaterThanOrEqual => FilterFunction.MoreThanOrEqual,
                    ExpressionType.LessThan => FilterFunction.LessThan,
                    ExpressionType.LessThanOrEqual => FilterFunction.LessThanOrEqual,
                    _ => throw new NotSupportedException(type + " is still not supported as a FilterFunction."),
                };
        }

        public static object GetValue(this Expression @this)
        {
            var result = @this.ExtractValue();
            if (result is IntEntity intEnt && intEnt.IsNew) return -1;
            if (result is Entity ent) return ent.GetId();
            return result;
        }

        static string GetColumnName(PropertyInfo property)
        {
            var name = property.Name;
            if (!name.EndsWith("Id") || NotAssociationAttribute.Marked(property)) return name;

            if (property.PropertyType.IsAnyOf(typeof(Guid), typeof(Guid?)))
                name = name.TrimEnd(2);

            return name;
        }

        public static PropertyInfo[] GetPropertiesInPath(this MemberExpression @this)
        {
            var empty = new PropertyInfo[0];

            // Handle the member:
            var property = @this.Member as PropertyInfo;
            if (property == null) return empty;

            // Fix for overriden properties:
            try { property = @this.Expression.Type.GetProperty(property.Name) ?? property; }
            catch
            {
                // No logging is needed
            }

            if (CalculatedAttribute.IsCalculated(property) || ComputedColumnAttribute.IsComputedColumn(property)) return empty;

            if (@this.Expression.IsSimpleParameter()) return new[] { property };

            if (@this.Expression is MemberExpression exp)
            {
                // The expression is itself a member of something.
                var parentProperty = exp.GetPropertiesInPath();
                if (parentProperty.None()) return empty;

                return parentProperty.Concat(property).ToArray();
            }

            if (@this.Expression.Type.IsNullable()) return new[] { property };

            return empty;
        }

        public static string GetDatabaseColumnPath(this MemberExpression @this)
        {
            var path = @this.GetPropertiesInPath();
            if (path.None()) return null;

            if (path.ExceptLast().Any(x => !x.PropertyType.Implements<IEntity>())) return null;

            return path.Select(GetColumnName).ToString(".");
        }
    }
}