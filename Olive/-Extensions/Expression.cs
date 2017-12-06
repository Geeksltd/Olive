namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Determines whether this property info is the specified property (in lambda expression).
        /// </summary>
        public static bool Is<T>(this PropertyInfo property, Expression<Func<T, object>> expression)
        {
            if (!typeof(T).IsA(property.DeclaringType)) return false;
            return expression.GetPropertyPath() == property.Name;
        }

        /// <summary>
        /// Gets the property name for a specified expression.
        /// </summary>
        public static MemberInfo GetMember<T, K>(this Expression<Func<T, K>> memberExpression)
        {
            var asMemberExpression = memberExpression.Body as MemberExpression;

            if (asMemberExpression == null)
            {
                // Maybe Unary:
                asMemberExpression = (memberExpression.Body as UnaryExpression)?.Operand as MemberExpression;
            }

            if (asMemberExpression == null) throw new Exception("Invalid expression");

            return asMemberExpression.Member;
        }

        public static PropertyInfo GetProperty<TModel, TProperty>(this Expression<Func<TModel, TProperty>> property) =>
            property.GetMember<TModel, TProperty>() as PropertyInfo;

        /// <summary>
        /// For example if the expression is (x => x.A.B) it will return A.B.
        /// </summary>
        public static string GetPropertyPath(this Expression expression)
        {
            if (expression is MemberExpression m)
            {
                var result = m.Member.Name;

                if (m.Expression.ToString().Contains("."))
                    result = m.Expression.GetPropertyPath() + "." + result;

                return result;
            }

            if (expression is LambdaExpression l) return l.Body.GetPropertyPath();

            if (expression is UnaryExpression u) return u.Operand.GetPropertyPath();

            throw new Exception("Failed to get the property name from this expression: " + expression);
        }
    }
}