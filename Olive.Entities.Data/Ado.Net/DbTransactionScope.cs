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

        /// <summary>
        /// Set once this root's transactions have been committed. Until the scope is disposed it is still the
        /// Root, and still lends its connections, but commands no longer join its transaction: they run on their
        /// own, as SQL Server's client always ran them (it drops a committed transaction from a command), whereas
        /// PostgreSQL's refuses a command given one ("Transaction is already completed").
        /// </summary>
        internal bool IsCommitted { get; private set; }

        /// <summary>The committed root this scope stands in for, when it was opened after that root's commit.</summary>
        readonly DbTransactionScope CommittedRoot;

        readonly List<WeakReference<IDataReader>> PotentiallyUnclosedReaders = new List<WeakReference<IDataReader>>();

        // Per unique connection string, one record is added to this.
        readonly Dictionary<string, (DbConnection Connection, DbTransaction Transaction)> Connections = new Dictionary<string, (DbConnection Connection, DbTransaction Transaction)>();

        public DbTransactionScope() : this(GetDefaultIsolationLevel()) { }

        public DbTransactionScope(DbTransactionScopeOption scopeOption) : this(GetDefaultIsolationLevel(), scopeOption) { }

        public DbTransactionScope(IsolationLevel isolationLevel, DbTransactionScopeOption scopeOption = DbTransactionScopeOption.Required)
        {
            IsolationLevel = isolationLevel;
            ScopeOption = scopeOption;

            // Nothing can join a transaction that has been committed: a scope opened after its root completed
            // (in the same using block) starts a transaction of its own, and hands the root back when disposed.
            if (Root?.IsCommitted == true)
            {
                CommittedRoot = Root;
                Root = null;
            }

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

        /// <summary>The root scope while its transaction is still open: null outside a scope, and after the
        /// root has been completed (committed) but not yet disposed.</summary>
        internal static DbTransactionScope Open => Root is DbTransactionScope root && !root.IsCommitted ? root : null;

        public Guid ID { get; } = Guid.NewGuid();

        #region TransactionCompletedEvent

        event EventHandler TransactionCompleted;

        /// <summary>
        /// Attaches an event handler to be invoked when the current (root) transaction is completed.
        /// Once it has been completed, the handler runs straight away: the event will not be raised again.
        /// </summary>
        public void OnTransactionCompleted(Action eventHandler)
        {
            if (Root?.IsCommitted == true) eventHandler?.Invoke();
            else Root.TransactionCompleted += (s, e) => eventHandler?.Invoke();
        }

        #endregion

        internal static IsolationLevel GetDefaultIsolationLevel() =>
             Config.Get("Default:Transaction:DefaultIsolationLevel", IsolationLevel.ReadUncommitted);

        internal async Task<DbTransaction> GetDbTransaction()
        {
            // After the commit there is no transaction to join (see IsCommitted).
            if (IsCommitted) return null;

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

                DbTransaction transaction = null;
                try
                {
                    // A connection first needed after the commit has nothing left to join (see IsCommitted).
                    if (!IsCommitted) transaction = connection.BeginTransaction(IsolationLevel);
                }
                catch
                {
                    connection.Close();
                    connection.Dispose();
                    throw;
                }

                Connections.Add(connectionString, (connection, transaction));
            }
        }

        /// <summary>
        /// Rolls back the transaction unless it was completed, then releases every connection in this scope.
        /// <para>
        /// This never throws. It usually runs while an exception is already unwinding, so each clean-up step
        /// is isolated and its failure logged rather than propagated — otherwise the original exception would
        /// be replaced and the remaining connections orphaned. Callers that need to know a roll-back failed
        /// should watch the error log; they will not see an exception from here.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            if (IsAborted) return;

            if (this == Root) // Root
            {
                Root = CommittedRoot;
                if (CommittedRoot != null) Current = CommittedRoot;

                if (!IsCompleted)
                {
                    // Root is not completed.
                    IsAborted = true;

                    Connections.Do(x => Release("roll back the transaction", () => x.Value.Transaction?.Rollback()));
                }

                // Always dispose transactions and connections.
                // Each step is isolated, because a failure in any one of them (a doomed transaction,
                // a connection already broken by a timeout or a server failover) must not prevent the
                // remaining connections from being returned to the pool. Otherwise they are orphaned
                // until the GC gets to them, which is how a slow database turns into pool exhaustion.
                Connections.Do(x =>
                {
                    Release("dispose the transaction", () => x.Value.Transaction?.Dispose());
                    Release("close the connection", () => x.Value.Connection.Close());
                    Release("dispose the connection", () => x.Value.Connection.Dispose());
                });

                Connections.Clear();
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

        /// <summary>
        /// Runs a clean-up step, logging rather than throwing on failure.
        /// Dispose() usually runs while an exception is already unwinding, so throwing from here
        /// would both replace the original exception and skip the remaining clean-up steps.
        /// </summary>
        void Release(string action, Action step)
        {
            try { step(); }
            catch (Exception ex)
            {
                Log.For(this).Error(ex, $"Failed to {action} of transaction scope {ID}.");
            }
        }

        public void Complete()
        {
            if (IsAborted)
                throw new Exception("This transaction is already aborted, probably due to a nested transaction not being completed.");

            IsCompleted = true;

            if (Root != this) return; // Ignore, and wait for the parent Completion.
            if (IsCommitted) return; // Already committed: there is nothing left to commit.

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

            // Before the handlers run, so that any reads or writes they make run on their own.
            IsCommitted = true;

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