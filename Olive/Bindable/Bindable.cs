namespace Olive
{
    using DebounceThrottle;
    using System;

    public abstract partial class Bindable : IBindable
    {
        object IBindable.Value { get => GetValue(); set => SetValue(value); }

        protected ThrottleDispatcher dispatcher = new(TimeSpan.FromSeconds(0));

        public abstract IBinding AddBinding(object target, string propertyName);

        protected abstract void SetValue(object value);

        protected abstract object GetValue();

        /// <summary>
        /// Rebinds it to the same value. Particularly useful for expression based bindings to get updated.
        /// </summary>
        public void Refresh() => SetValue(GetValue());

        public override string ToString() => GetValue().ToStringOrEmpty();

        public abstract void ClearBindings();
    }
}