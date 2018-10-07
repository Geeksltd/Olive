using System.Transactions;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides extension methods for transaction classes.
    /// </summary>
    public static class TransactionExtensions
    {
        /// <summary>
        /// Creates a new transaction scope with this isolation level.
        /// </summary>
        public static TransactionScope CreateScope(this IsolationLevel @this) =>
            CreateScope(@this, TransactionScopeOption.Required);

        /// <summary>
        /// Creates a new transaction scope with this isolation level.
        /// </summary> public static TransactionScope CreateScope(this IsolationLevel isolationLevel, TransactionScopeOption scopeOption)
        [EscapeGCop("I am the solution to this GCop warning")]
        public static TransactionScope CreateScope(this IsolationLevel @this, TransactionScopeOption scopeOption)
        {
            var options = new TransactionOptions
            {
                IsolationLevel = @this,
                Timeout = TransactionManager.DefaultTimeout
            };
            return new TransactionScope(scopeOption, options);
        }
    }
}
