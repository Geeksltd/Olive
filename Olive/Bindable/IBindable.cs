namespace Olive
{
    /// <summary>
    /// Provides a wrapper around a property value which can be used in data binding.
    /// </summary>
    public interface IBindable
    {
        IBinding AddBinding(object target, string propertyName);
        object Value { get; set; }
        void Refresh();
    }
}