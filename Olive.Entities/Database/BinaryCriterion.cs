namespace Olive.Entities
{
    public enum BinaryOperator { OR, AND }

    public class BinaryCriterion : Criterion
    {
        BinaryCriterion() : base("N/A", "N/A") { }

        BinaryCriterion(Criterion left, BinaryOperator opt, Criterion right)
           : base("N/A", "N/A")
        {
            Left = left;
            Right = right;
            Operator = opt;
        }

        public Criterion Left { get; set; }
        public Criterion Right { get; set; }
        public BinaryOperator Operator { get; set; }
        internal bool IsConvertedCompletely { get; set; } = true;

        public BinaryCriterion Or(BinaryCriterion left, BinaryCriterion right) => new BinaryCriterion(left, BinaryOperator.OR, right);

        public BinaryCriterion And(BinaryCriterion left, BinaryCriterion right) => new BinaryCriterion(left, BinaryOperator.AND, right);

        public static BinaryCriterion From<T>(BinaryExpression expression) where T : IEntity => CreateByExpression<T>(expression);

        static Criterion CreateByExpression<T>(Expression expression) where T : IEntity =>
            CriteriaExtractor<T>.CreateCriterion(expression);

        static BinaryCriterion CreateByExpression<T>(BinaryExpression expression) where T : IEntity
        {
            Criterion left, right;

            if (expression.Left.NodeType == ExpressionType.OrElse || expression.Left.NodeType == ExpressionType.AndAlso)
                left = CreateByExpression<T>(expression.Left as BinaryExpression);
            else
                left = CreateByExpression<T>(expression.Left);

            if (expression.Right.NodeType == ExpressionType.OrElse || expression.Right.NodeType == ExpressionType.AndAlso)
                right = CreateByExpression<T>(expression.Right as BinaryExpression);
            else
                right = CreateByExpression<T>(expression.Right);

            if (left == null || right == null)
                throw new System.Exception("Failed to convert the specified expression to Criterion: " + expression);

            var op = (expression.NodeType == ExpressionType.OrElse ? BinaryOperator.OR : BinaryOperator.AND);
            return new BinaryCriterion(left, op, right);
        }

        public override string ToString()
        {
            if (SqlCondition.HasValue()) return SqlCondition;
            return $"({Left}-{Operator}-{Right})";
        }
    }
}