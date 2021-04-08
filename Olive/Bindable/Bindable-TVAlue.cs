namespace Olive
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides a wrapper around a property value which can be used in data binding.
    /// </summary>
    public partial class Bindable<TValue> : Bindable
    {
        TValue value;
        readonly ConcurrentList<IBinding<TValue>> Bindings = new();

        /// <summary>
        /// Fired when the value is changed by either source (API) or user input.
        /// </summary>
        public event Action Changed;

        public Bindable() { }

        public Bindable(TValue value) => this.value = value;

        public TValue Value
        {
            get => value;
            set => SetValue(value);
        }

        /// <summary>
        /// Sets the value by source API and fires the Changed (and ChangedBySource) events. 
        /// </summary>
        public virtual void Set(TValue newValue) => SetValue(newValue);

        protected override void SetValue(object value)
        {
            this.value = (TValue)value;
            foreach (var item in Bindings) item.Apply((TValue)value);
            Changed?.Invoke();
        }

        protected override object GetValue() => Value;

        public static implicit operator Bindable<TValue>(TValue value) => new Bindable<TValue>(value);

        public static implicit operator TValue(Bindable<TValue> item) => item.value;

        public static bool operator ==(Bindable<TValue> item, object other)
        {
            if (item is null)
                throw new InvalidOperationException("A bindable object should never be null. It should be instantiated upon declaration.");

            if (other is IBindable bindable)
                other = bindable.Value;

            if (other is null) return item.value is null;
            if (item.Value is null) return false;
            return item.Value.Equals(other);
        }

        public static bool operator !=(Bindable<TValue> item, object other) => !(item == other);

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

            var result = Bindings.FirstOrDefault(x => x.Equals(binding));
            if (!(result is null)) return result;

            Bindings.Add(binding);
            binding.Apply(value);

            if (target is IBindableInput input)
            {
                void Input_InputChanged(string changedProperty)
                {
                    if (changedProperty == propertyName)
                        SeValuetByInput((TValue)binding.Property.GetValue(target));
                }

                input.InputChanged += Input_InputChanged;
            }

            return binding;
        }

        internal virtual void SeValuetByInput(TValue value) => SetValue(value);

        public IBinding<TValue> AddBinding<TProperty>(object target, string propertyName, Func<TValue, TProperty> expression)
        {
            var property = FindProperty(target, propertyName);

            var binding = new PropertyBinding<TValue>
            {
                Target = target.GetWeakReference(),
                Property = property,
                Expression = x => (object)expression(x)
            };

            var result = Bindings.FirstOrDefault(x => x.Equals(binding));
            if (result is null)
            {
                Bindings.Add(binding);
                binding.Apply(value);
                return binding;
            }
            else
            {
                if (binding.IsRemoved) binding.IsRemoved = false;
                return result;
            }
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

        public override void ClearBindings() => Changed = null;
    }
}