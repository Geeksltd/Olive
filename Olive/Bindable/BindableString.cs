using Olive;
using System;
using System.Linq;

namespace Olive
{
    public class BindableString : Bindable<string>
    {
        public readonly Bindable<string> State = new();

        public BindableString(TimeSpan timeout) : base(timeout) => State.Bind(this, x => "skeleton".OnlyWhen(x.HasValue() && x.All(c => c.IsAnyOf(' ', '▅'))));

        public BindableString() : base() => State.Bind(this, x => "skeleton".OnlyWhen(x.HasValue() && x.All(c => c.IsAnyOf(' ', '▅'))));
    }
}