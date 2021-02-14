using System;

namespace Olive
{
    public class ExpressionBindable<TOut, TParam1> : Bindable<TOut>
    {
        Bindable<TParam1> Source;
        Func<TParam1, TOut> Expression;
        public ExpressionBindable(Bindable<TParam1> source, Func<TParam1, TOut> expression)
        {
            Source = source;
            Expression = expression;
            this.RefreshOn(source);
        }

        protected override object GetValue() => Expression(Source.Value);
    }
}
