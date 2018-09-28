namespace Olive.Entities
{
    /// <summary>
    /// Provides an abstraction for database query criteria.
    /// </summary>
    public interface ICriterion
    {
        string PropertyName { get; }

        FilterFunction FilterFunction { get; set; }
        object Value { get; }

        string SqlCondition { get; }
    }
}