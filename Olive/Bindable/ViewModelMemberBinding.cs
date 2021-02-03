namespace Olive
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Olive;

    partial class Bindable<TValue>
    {
        public class ViewModelMemberBinding<TMember>
        {
            readonly MemberInfo Member;
            readonly Bindable<TValue> ViewModel;
            public ViewModelMemberBinding(Bindable<TValue> viewModel, Expression<Func<TValue, TMember>> viewModelMember)
            {
                ViewModel = viewModel ?? throw new ArgumentNullException(nameof(ViewModel));

                Member = viewModelMember.GetMember();
                if (Member is null) throw new Exception("No such public field or property was found on " + typeof(TValue).Name);
            }

            public Task Update(TMember value)
            {
                var updated = ViewModel.Value;
                Member.SetValue(updated, value);
                ViewModel.Set(updated);
                return Task.CompletedTask;
            }
        }
    }
}