namespace Olive.Entities
{
    public interface ITransactionScope : IDisposable
    {
        void Complete();

        Guid ID { get; }
    }
}
