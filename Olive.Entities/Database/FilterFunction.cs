using System;

namespace Olive.Entities
{
    /// <summary>
    /// Provides options for filter functions.
    /// </summary>
    public enum FilterFunction
    {
        Is,
        IsNot,
        Null,
        NotNull,

        Contains,
        NotContains,
        ContainsAll,
        ContainsAny,

        In,
        NotIn,
        BeginsWith,
        EndsWith,
        InRange,

        LessThan,
        LessThanOrEqual,
        MoreThan,
        MoreThanOrEqual
    }

    public static class FilterFunctionServices
    {
        /// <summary>
        /// Gets the database operator equivalent for this filter option.
        /// </summary>
        public static string GetDatabaseOperator(this FilterFunction option)
        {
            switch (option)
            {
                case FilterFunction.Contains:
                case FilterFunction.BeginsWith:
                case FilterFunction.EndsWith:
                    return "LIKE";
                case FilterFunction.Is:
                    return "=";
                case FilterFunction.IsNot:
                    return "<>";
                case FilterFunction.LessThan:
                    return "<";
                case FilterFunction.LessThanOrEqual:
                    return "<=";
                case FilterFunction.MoreThan:
                    return ">";
                case FilterFunction.MoreThanOrEqual:
                    return ">=";
                case FilterFunction.Null:
                    return "Is NULL";
                case FilterFunction.In:
                    return "IN";
                case FilterFunction.NotIn:
                    return "NOT IN";
                case FilterFunction.NotNull:
                    return "Is NOT NULL";
                case FilterFunction.NotContains:
                    return "NOT LIKE";
                default:
                    throw new NotSupportedException(option + " is not supported in GetDatabaseOperator().");
            }
        }
    }
}