using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    // TODO: If it's a Suppress, then simply in the GetDbTransaction return null.
    // And test to see if the command will pass in case where other commands in a transaction in the same connection exist, 
    // and are rolled back.

    public class DbTransactionScope : ITransactionScope
    {
        IsolationLevel IsolationLevel;
        DbTransactionScopeOption ScopeOption;

        public static DbTransactionScope Root
        {
            get => CallContext<DbTransactionScope>.GetData(nameof(Root));
            set => CallContext<DbTransactionScope>.SetData(nameof(Root), value);
        }

        public static DbTransactionScope Current
        {
            get => CallContext<DbTransactionScope>.GetData(nameof(Current));
            set => CallContext<DbTransactionScope>.SetData(nameof(Current), value);
        }

        public static DbTransactionScope Parent
        {
            get => CallContext<DbTransactionScope>.GetData(nameof(Parent));
            set => CallContext<DbTransactionScope>.SetData(nameof(Parent), value);
        }

        // Per unique connection string, one record is added to this.
        Dictionary<string, Tuple<DbConnection, DbTransaction>> Connections = new Dictionary<string, Tuple<DbConnection, DbTransaction>>();

        bool IsCompleted, IsAborted;

        public DbTransactionScope() : this(GetDefaultIsolationLevel()) { }

        public DbTransactionScope(DbTransactionScopeOption scopeOption) : this(GetDefaultIsolationLevel(), scopeOption) { }

        public DbTransactionScope(IsolationLevel isolationLevel, DbTransactionScopeOption scopeOption = DbTransactionScopeOption.Required)
        {
            IsolationLevel = isolationLevel;
            ScopeOption = scopeOption;

            Parent = Root;
            Current = this;

            if (Root == null) Root = this;
        }

        public Guid ID { get; } = Guid.NewGuid();

        #region TransactionCompletedEvent

        event EventHandler TransactionCompleted;

        /// <summary>
        /// Attaches an event handler to be invoked when the current (root) transaction is completed.
        /// </summary>
        public void OnTransactionCompleted(Action eventHandler) => Root.TransactionCompleted += (s, e) => eventHandler?.Invoke();

        #endregion

        static IsolationLevel GetDefaultIsolationLevel() =>
            Config.Get("Default.Transaction.IsolationLevel", IsolationLevel.ReadUncommitted);

        internal async Task<DbTransaction> GetDbTransaction()
        {
            var connectionString = DataAccess.GetCurrentConnectionString();

            await Setup(connectionString);

            return Connections[connectionString].Item2;
        }

        internal async Task<IDbConnection> GetDbConnection()
        {
            var connectionString = DataAccess.GetCurrentConnectionString();

            await Setup(connectionString);

            return Connections[connectionString].Item1;
        }

        async Task Setup(string connectionString)
        {
            if (Connections.LacksKey(connectionString))
            {
                var access = Database.Instance.GetAccess(connectionString);

                var connection = (DbConnection)await access.CreateConnection();
                var transaction = connection.BeginTransaction(IsolationLevel);

                Connections.Add(connectionString, Tuple.Create(connection, transaction));
            }
        }

        public void Dispose()
        {
            if (IsAborted) return;

            if (this == Root) // Root
            {
                Root = null;

                if (IsCompleted)
                {
                    // Happy scenario:
                    Connections.Do(x => x.Value.Item1.Close());
                }
                else // Root is not completed.
                {
                    IsAborted = true;

                    Connections.Do(x => x.Value.Item2.Rollback());
                    Connections.Do(x => x.Value.Item2.Dispose());
                    Connections.Do(x => x.Value.Item1.Close());
                }
            }
            else
            {
                Current = Parent;

                if (IsCompleted)
                {
                    // A Sub-transaction has been happily completed.
                    // Just wait for the parent.
                }
                else
                {
                    // A sub transaction is not completed.
                    Root?.Dispose();
                }
            }
        }

        public void Complete()
        {
            if (IsAborted)
                throw new Exception("This transaction is already aborted, probably due to a nested transaction not being completed.");

            IsCompleted = true;

            if (Root == this)
            {
                // I'm the root:
                foreach (var item in Connections)
                    item.Value.Item2.Commit();

                TransactionCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Ignore, and wait for the parent Completion.
            }
        }
    }
}