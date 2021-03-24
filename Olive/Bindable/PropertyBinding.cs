namespace Olive
{
    using System;
    using System.Reflection;

    internal struct PropertyBinding<TValue> : IBinding<TValue>
    {
        public WeakReference<object> Target;
        public PropertyInfo Property;
        public Func<TValue, object> Expression;
        public bool IsRemoved;

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
                else if (Property.PropertyType == typeof(string) && !(settableValue is string))
                    settableValue = settableValue.ToStringOrEmpty();
                else if (!Property.PropertyType.IsAssignableFrom(settableValue.GetType()))
                    settableValue = Convert.ChangeType(settableValue, Property.PropertyType);

                Property.SetValue(target, settableValue);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to apply the binding value of '{value}' on the {Property.Name} property of {target.GetType().FullName}", ex);
            }
        }

        public void Remove()
        {
            IsRemoved = true; // How about clean up?
        }

        object GetSettableValue(TValue rawValue)
        {
            if (Expression != null)
                return Expression.Invoke(rawValue);

            return rawValue;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PropertyBinding<TValue> binding)) return false;

            if (binding.Property != Property) return false;
            if (!binding.Target.TryGetTarget(out var target)) return false;
            if (target is null) return false;

            if (Target.TryGetTarget(out var myTarget))
                return ReferenceEquals(myTarget, target);

            return false;
        }
    }
}