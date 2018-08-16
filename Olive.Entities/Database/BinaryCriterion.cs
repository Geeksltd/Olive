using System.Linq.Expressions;

namespace Olive.Entities
{
    public enum BinaryOperator { OR, AND }

    public class BinaryCriterion : Criterion
    {
        BinaryCriterion() : base("N/A", "N/A") { }

        public BinaryCriterion(Criterion left, BinaryOperator opt, Criterion right) : base("N/A", "N/A")
        {
            Left = left;
            Right = right;
            Operator = opt;
        }

        public Criterion Left { get; set; }

        public Criterion Right { get; set; }

        public BinaryOperator Operator { get; set; }

        internal bool IsConvertedCompletely { get; set; } = true;

        public override string ToString()
        {
            if (SqlCondition.HasValue()) return SqlCondition;
            return $"({Left}-{Operator}-{Right})";
        }

        public BinaryCriterion Or(BinaryCriterion left, BinaryCriterion right)
        {
            return new BinaryCriterion(left, BinaryOperator.OR, right);
        }

        public BinaryCriterion And(BinaryCriterion left, BinaryCriterion right)
        {
            return new BinaryCriterion(left, BinaryOperator.AND, right);
        }

        internal new static BinaryCriterion From(BinaryExpression expression)
        {
            var left = From(expression.Left);
            var right = From(expression.Right);

            if (left == null && right == null) return null;
            if (((left == null) != (right == null)))
            {
                // Only one of them has value.
                if (expression.NodeType == ExpressionType.AndAlso)
                {
                    return new BinaryCriterion(left, BinaryOperator.AND, right) { IsConvertedCompletely = false };
                }
                else
                {
                    // OR scenario. If one of them isn't converted, then there is no point in evaluating the other one.
                    return null;
                }
            }
            else
            {
                var op = (expression.NodeType == ExpressionType.OrElse ? BinaryOperator.OR : BinaryOperator.AND);
                return new BinaryCriterion(left, op, right);
            }
        }
    }
}