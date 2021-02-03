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
            return option switch
            {
                FilterFunction.Contains or FilterFunction.BeginsWith or FilterFunction.EndsWith => "LIKE",
                FilterFunction.Is => "=",
                FilterFunction.IsNot => "<>",
                FilterFunction.LessThan => "<",
                FilterFunction.LessThanOrEqual => "<=",
                FilterFunction.MoreThan => ">",
                FilterFunction.MoreThanOrEqual => ">=",
                FilterFunction.Null => "Is NULL",
                FilterFunction.In => "IN",
                FilterFunction.NotIn => "NOT IN",
                FilterFunction.NotNull => "Is NOT NULL",
                FilterFunction.NotContains => "NOT LIKE",
                _ => throw new NotSupportedException(option + " is not supported in GetDatabaseOperator()."),
            };
        }
    }
}