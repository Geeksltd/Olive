using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Rejects database query expressions that refer to properties which are not stored.
    /// Captured objects and constants are not database properties.
    /// </summary>
    internal sealed class CalculatedPropertyExpressionValidator : ExpressionVisitor
    {
        readonly string operation;

        CalculatedPropertyExpressionValidator(string operation) => this.operation = operation;

        public static void Validate(Expression expression, string operation) =>
            new CalculatedPropertyExpressionValidator(operation).Visit(expression);

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo property && IsEntityMember(node))
            {
                var actualProperty = node.Expression.Type.GetProperty(property.Name) ?? property;
                DatabaseQuery.ThrowIfCalculated(actualProperty, operation);
            }

            return base.VisitMember(node);
        }

        static bool IsEntityMember(MemberExpression member)
        {
            var expression = member.Expression;

            while (expression is MemberExpression parent)
                expression = parent.Expression;

            while (expression is UnaryExpression unary &&
                (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                expression = unary.Operand;

            return expression is ParameterExpression;
        }
    }
}
