namespace Olive.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class CriteriaExtractor<T> where T : IEntity
    {
        Expression<Func<T, bool>> Criteria;
        bool ThrowOnNonConversion;
        public List<Expression> NotConverted = new List<Expression>();

        public CriteriaExtractor(Expression<Func<T, bool>> criteria, bool throwOnNonConversion)
        {
            Criteria = criteria;
            ThrowOnNonConversion = throwOnNonConversion;
        }

        public List<Criterion> Extract()
        {
            var result = new List<Criterion>();

            foreach (var ex in GetUnitExpressions(Criteria.Body))
            {
                var condition = Criterion.From(ex);

                if (condition == null || (condition as BinaryCriterion)?.IsConvertedCompletely == false)
                {
                    if (ThrowOnNonConversion)
                        throw new Exception("Failed to extract a criterion from expression: " + ex);

                    NotConverted.Add(ex);
                }

                result.Add(condition);
            }

            return result;
        }

        static IEnumerable<Expression> GetUnitExpressions(Expression expression)
        {
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                var binary = expression as BinaryExpression;
                return GetUnitExpressions(binary.Left).Concat(GetUnitExpressions(binary.Right));
            }

            else return new[] { expression };
        }
    }
}