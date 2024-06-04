namespace Olive
{
    using System;
    using System.Reflection;

    public interface IMortal
    {
        bool IsDead();
    }

    internal struct PropertyBinding<TValue> : IBinding<TValue>
    {
        Bindable<TValue> Owner;

        public WeakReference<object> Target;
        public PropertyInfo Property;
        public Func<TValue, object> Expression;

        bool IsRemoved;

        public PropertyBinding(Bindable<TValue> owner) => Owner = owner;

        public void Apply(TValue value)
        {
            if (IsRemoved) return;

            var target = Target.GetTargetOrDefault();
            if (target is null) return;

            try
            {
                var settableValue = GetSettableValue(value);

                if (settableValue == null)
                    settableValue = Property.PropertyType.GetDefaultValue();
                else if (Property.PropertyType == typeof(string) && settableValue is not string)
                    settableValue = settableValue.ToStringOrEmpty();
                else if (!Property.PropertyType.IsAssignableFrom(settableValue.GetType()))
                    settableValue = Convert.ChangeType(settableValue, Property.PropertyType);

                Property.SetValue(target, settableValue);
            }
            catch (Exception ex)
            {
                throw new($"Failed to apply the binding value of '{value}' on the {Property.Name} property of {target.GetType().FullName}", ex);
            }
        }

        public void Remove()
        {
            Owner.RemoveBinding(this);
            IsRemoved = true;
        }

        public bool IsDead()
        {
            if (IsRemoved) return true;
            if (!Target.TryGetTarget(out var target)) return true;
            if (target is IMortal m && m.IsDead()) return true;
            return false;
        }

        object GetSettableValue(TValue rawValue)
        {
            if (Expression != null)
                return Expression.Invoke(rawValue);

            return rawValue;
        }

        internal bool SameTarget(PropertyBinding<TValue> binding)
        {
            if (binding.Property != Property) return false;
            if (!binding.Target.TryGetTarget(out var target)) return false;
            if (!Target.TryGetTarget(out var myTarget)) return false;

            return ReferenceEquals(myTarget, target);
        }
    }
}