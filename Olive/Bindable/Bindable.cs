namespace Olive
{
    using System;
    using System.Reflection;

    public abstract partial class Bindable : IBindable
    {
        object IBindable.Value { get => GetValue(); set => SetValue(value); }

        public abstract IBinding AddBinding(object target, string propertyName);

        public void SetUserValue(object value) => SetValue(value, byUserInput: true);

        protected abstract void SetValue(object value, bool byUserInput = false);

        protected abstract object GetValue();

        /// <summary>
        /// Rebinds it to the same value. Particularly useful for expression based bindings to get updated.
        /// </summary>
        public void Refresh() => SetValue(GetValue());
    }

    /// <summary>
    /// Provides a wrapper around a property value which can be used in data binding.
    /// </summary>
    public partial class Bindable<TValue> : Bindable
    {
        TValue value;
        readonly ConcurrentList<IBinding<TValue>> Bindings = new ConcurrentList<IBinding<TValue>>();

        public event Action Changed;

        public Bindable() { }

        public Bindable(TValue value) => this.value = value;

        public TValue Value
        {
            get => value;
            set => SetValue(value);
        }

        public void Set(TValue newValue) => SetValue(newValue);

        protected override void SetValue(object value, bool byUserInput = false)
        {
            this.value = (TValue)value;

            if (!byUserInput) Changed?.Invoke();
            foreach (var item in Bindings) item.Apply((TValue)value);
        }

        protected override object GetValue() => Value;

        public static implicit operator Bindable<TValue>(TValue value) => new Bindable<TValue>(value);
        public static implicit operator TValue(Bindable<TValue> item) => item.value;

        /// <summary>
        /// Binds this instance to another bindable source. Use it to cascade data bininding.
        /// </summary>
        public void Bind<TSource>(Bindable<TSource> source, Func<TSource, TValue> expression)
        {
            source.AddBinding(this, nameof(Value), expression);
        }

        /// <summary>
        /// Binds a specified object's property this this instance, so every time my value is changed, it will be applied automatically on the target.
        /// </summary>
        /// <param name="target">The target object whose property should be bound to my value.</param>
        /// <param name="propertyName">The property of the target object that should have my value.</param>
        public override IBinding AddBinding(object target, string propertyName)
        {
            var property = FindProperty(target, propertyName);

            var binding = new PropertyBinding<TValue> { Target = target.GetWeakReference(), Property = property };
            Bindings.Add(binding);
            binding.Apply(value);

            if (target is IBindableInput inp)
                inp.AddBinding(this);

            return binding;
        }

        public IBinding<TValue> AddBinding<TProperty>(object target, string propertyName, Func<TValue, TProperty> expression)
        {
            var property = FindProperty(target, propertyName);

            var binding = new PropertyBinding<TValue>
            {
                Target = target.GetWeakReference(),
                Property = property,
                Expression = x => (object)expression(x)
            };

            Bindings.Add(binding);
            binding.Apply(value);

            return binding;
        }

        static PropertyInfo FindProperty(object target, string propertyName)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (propertyName.IsEmpty()) throw new ArgumentNullException(nameof(propertyName));

            var property = target.GetType().GetProperty(propertyName);
            if (property is null)
                throw new Exception($"{target.GetType().FullName} does not have a public instance property named {propertyName}.");

            if (!property.CanWrite)
                throw new Exception($"The {propertyName} property of {target.GetType().FullName} is read-only.");

            return property;
        }
    }
}