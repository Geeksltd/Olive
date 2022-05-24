using System;

namespace Olive
{
    /// <summary>
    /// A bindable abstraction with specialised events to distinguish between changes made by the user, and by the API, to prevent infinite loops, and enable proper cascading.
    /// </summary>
    public class TwoWayBindable<TValue> : Bindable<TValue>
    {
        public TwoWayBindable() : base() { }

        public TwoWayBindable(TValue defaultValue) : base(defaultValue) { }

        /// <summary>
        /// Fired when the value is changed by input (user action) rather than source (API).
        /// </summary>
        public event Action ChangedByInput;

        /// <summary>
        /// Fired when the value is changed by source (API) rather than a user input.
        /// </summary>
        public event Action ChangedBySource;

        /// <summary>
        /// Sets the value by user and fires the ChangedByInput event. 
        /// </summary>
        public void SetByInput(TValue value)
        {
            base.SetValue(value);
            ChangedByInput?.Invoke();
        }

        protected override void SetValue(object value)
        {
            base.SetValue(value);
            ChangedBySource?.Invoke();
        }

        internal override void SeValueByInput(TValue value) => SetByInput(value);

        public override void ClearBindings()
        {
            base.ClearBindings();
            ChangedByInput = null;
            ChangedBySource = null;
        }
    }
}