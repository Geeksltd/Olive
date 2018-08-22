using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Determines whether this property info is the specified property (in lambda expression).
        /// </summary>
        public static bool Is<T>(this PropertyInfo @this, Expression<Func<T, object>> expression)
        {
            if (!typeof(T).IsA(@this.DeclaringType)) return false;
            return expression.GetPropertyPath() == @this.Name;
        }

        /// <summary>
        /// Gets the property name for a specified expression.
        /// </summary>
        public static MemberInfo GetMember<T, K>(this Expression<Func<T, K>> @this)
        {
            var asMemberExpression = @this.Body as MemberExpression;

            if (asMemberExpression == null)
            {
                // Maybe Unary:
                asMemberExpression = (@this.Body as UnaryExpression)?.Operand as MemberExpression;
            }

            if (asMemberExpression == null) throw new Exception("Invalid expression");

            return asMemberExpression.Member;
        }

        public static PropertyInfo GetProperty<TModel, TProperty>(this Expression<Func<TModel, TProperty>> @this) =>
            @this.GetMember<TModel, TProperty>() as PropertyInfo;

        /// <summary>
        /// For example if the expression is (x => x.A.B) it will return A.B.
        /// </summary>
        public static string GetPropertyPath(this Expression @this)
        {
            if (@this is MemberExpression m)
            {
                var result = m.Member.Name;

                if (m.Expression.ToString().Contains("."))
                    result = m.Expression.GetPropertyPath() + "." + result;

                return result;
            }

            if (@this is LambdaExpression l) return l.Body.GetPropertyPath();

            if (@this is UnaryExpression u) return u.Operand.GetPropertyPath();

            throw new Exception("Failed to get the property name from this expression: " + @this);
        }

        public static object CompileAndInvoke(this Expression @this)
        {
            return Expression.Lambda(typeof(Func<>).MakeGenericType(@this.Type),
                @this).Compile().DynamicInvoke();
        }

        public static object ExtractValue(this Expression @this)
        {
            if (@this == null) return null;

            if (@this is ConstantExpression)
                return (@this as ConstantExpression).Value;

            if (@this is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;

                if (member is PropertyInfo prop)
                    return prop.GetValue(memberExpression.Expression.ExtractValue());

                else if (member is FieldInfo field)
                    return field.GetValue(memberExpression.Expression.ExtractValue());

                else
                    return @this.CompileAndInvoke();
            }
            else if (@this is MethodCallExpression methodExpression)
            {
                var method = methodExpression.Method;
                var instance = methodExpression.Object.ExtractValue();

                return method.Invoke(instance, methodExpression.Arguments.Select(a => ExtractValue(a)).ToArray());
            }
            else
            {
                return @this.CompileAndInvoke();
            }
        }

        public static bool IsSimpleParameter(this Expression @this)
        {
            if (@this is ParameterExpression) return true;

            if (@this is UnaryExpression && (@this.NodeType == ExpressionType.Convert))
                return true;

            return false;
        }
    }
}