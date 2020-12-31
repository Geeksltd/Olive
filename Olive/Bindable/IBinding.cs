namespace Zebble
{
    public interface IBinding
    {
        void Remove();
    }

    interface IBinding<TValue> : IBinding
    {
        void Apply(TValue value);
    }
}