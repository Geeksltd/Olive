namespace Zebble
{
    public interface IBinding
    {
        void Remove();
    }

    public interface IBinding<TValue> : IBinding
    {
        void Apply(TValue value);
    }
}