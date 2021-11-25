using System;
using System.ComponentModel;
using System.Transactions;

namespace Olive.Entities.Data
{
    // public interface ITransactionScope : IDisposable
    // {
    //    void Complete();

    //    Guid ID { get; }
    // }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TransactionScopeWrapper : ITransactionScope
    {
        TransactionScope Scope;

        public Guid ID { get; } = Guid.NewGuid();

        public TransactionScopeWrapper(TransactionScope scope) => Scope = scope;

        public void Complete() => Scope.Complete();

        public void Dispose() => Scope.Dispose();
    }
}