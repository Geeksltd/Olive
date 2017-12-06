namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property of type DateTime or Nullable[DateTime] it specifies that values are for Date only, 
    /// and the time part is meant to be disregarded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class DateOnlyAttribute : Attribute { }
}