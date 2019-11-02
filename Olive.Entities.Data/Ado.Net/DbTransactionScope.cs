using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    // TODO: If it's a Suppress, then simply in the GetDbTransaction return null.
    // And test to see if the command will pass in case where other commands in a transaction in the same connection exist, 
    // and are rolled back.

    public class DbTransactionScope : ITransactionScope
    {
        readonly IsolationLevel IsolationLevel;
        readonly DbTransactionScopeOption ScopeOption;
        bool IsCompleted, IsAborted;
        readonly List<WeakReference<IDataReader>> PotentiallyUnclosedReaders = new List<WeakReference<IDataReader>>();

        // Per unique connection string, one record is added to this.
        readonly Dictionary<string, (DbConnection Connection, DbTransaction Transaction)> Connections = new Dictionary<string, (DbConnection Connection, DbTransaction Transaction)>();

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

        public Guid ID { get; } = Guid.NewGuid();

        #region TransactionCompletedEvent

        event EventHandler TransactionCompleted;

        /// <summary>
        /// Attaches an event handler to be invoked when the current (root) transaction is completed.
        /// </summary>
        public void OnTransactionCompleted(Action eventHandler) => Root.TransactionCompleted += (s, e) => eventHandler?.Invoke();

        #endregion

        internal static IsolationLevel GetDefaultIsolationLevel() =>
             Config.Get("Default:Transaction:DefaultIsolationLevel", IsolationLevel.ReadUncommitted);

        internal async Task<DbTransaction> GetDbTransaction()
        {
            var connectionString = DataAccess.GetCurrentConnectionString();
            await Setup(connectionString);
            return Connections[connectionString].Transaction;
        }

        internal async Task<IDbConnection> GetDbConnection()
        {
            var connectionString = DataAccess.GetCurrentConnectionString();
            await Setup(connectionString);
            return Connections[connectionString].Connection;
        }

        async Task Setup(string connectionString)
        {
            if (Connections.LacksKey(connectionString))
            {
                var access = Context.Current.Database().GetAccess(connectionString);
                var connection = (DbConnection)await access.CreateConnection();
                var transaction = connection.BeginTransaction(IsolationLevel);

                Connections.Add(connectionString, (connection, transaction));
            }
        }

        public void Dispose()
        {
            if (IsAborted) return;

            if (this == Root) // Root
            {
                Root = null;

                if (!IsCompleted)
                {
                    // Root is not completed.
                    IsAborted = true;
                    Connections.Do(x => x.Value.Transaction.Rollback());
                    Connections.Do(x => x.Value.Transaction.Dispose());
                }

                Connections.Do(x => x.Value.Connection.Close());
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

            if (Root != this) return; // Ignore, and wait for the parent Completion.

            foreach (var reader in PotentiallyUnclosedReaders.Select(x => x.GetTargetOrDefault()).ExceptNull())
                if (!reader.IsClosed)
                {
                    reader.Close();
                    reader.Dispose();
                }

            foreach (var item in Connections)
            {
                var retries = 1;
                while (AsyncCommandInProgress(item.Value.Connection))
                {
                    Thread.Sleep(retries * 10);
                    if (retries++ > 10)
                        throw new Exception("Async command is in progress in this transaction.");
                }

                item.Value.Transaction.Commit();
            }

            TransactionCompleted?.Invoke(this, EventArgs.Empty);
        }

        static bool AsyncCommandInProgress(IDbConnection connection)
        {
            var property =
            connection.GetType().GetProperty("AsyncCommandInProgress", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (property == null) return false;

            return (bool)property.GetValue(connection);
        }

        internal void Register(DbDataReader reader)
            => PotentiallyUnclosedReaders.Add(new WeakReference<IDataReader>(reader));
    }
}