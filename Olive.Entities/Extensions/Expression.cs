using Olive.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive
{
    partial class OliveEntitiesExtensions
    {
        public static FilterFunction ToFilterFunction(this ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal: return FilterFunction.Is;
                case ExpressionType.NotEqual: return FilterFunction.IsNot;
                case ExpressionType.GreaterThan: return FilterFunction.MoreThan;
                case ExpressionType.GreaterThanOrEqual: return FilterFunction.MoreThanOrEqual;
                case ExpressionType.LessThan: return FilterFunction.LessThan;
                case ExpressionType.LessThanOrEqual: return FilterFunction.LessThanOrEqual;
                default: throw new NotSupportedException(type + " is still not supported as a FilterFunction.");
            }
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
            if (!name.EndsWith("Id")) return name;
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
            catch { }

            if (CalculatedAttribute.IsCalculated(property)) return empty;

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