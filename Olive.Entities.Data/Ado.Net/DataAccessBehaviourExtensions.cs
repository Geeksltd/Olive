using System;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Behaviours that differ from one database to another, each answered by the provider's
    /// <see cref="SqlCommandGenerator"/>, so code calling them names no provider.
    /// </summary>
    public static class DataAccessBehaviourExtensions
    {
        const string BEST_EFFORT_SAVEPOINT = "olive_best_effort";

        /// <summary>
        /// Runs a write that is allowed to fail without costing the rest of the current transaction, and says
        /// whether it succeeded. Its exception is swallowed: the caller decides whether a failure is worth logging.
        /// <para>
        /// Inside a <see cref="DbTransactionScope"/> the write goes behind a savepoint that a failure rolls back
        /// to. On PostgreSQL a failed statement otherwise aborts the whole transaction, so that everything after
        /// it fails too; on SQL Server the transaction survives, and the savepoint undoes whatever part of a
        /// several-statement write had run. Outside a transaction each statement stands alone, so a failure is
        /// only caught.
        /// </para>
        /// </summary>
        public static async Task<bool> BestEffort(this IDataAccess @this, Func<Task> write)
        {
            var generator = @this.GetSqlCommandGenerator() as SqlCommandGenerator;

            if (DbTransactionScope.Open == null || generator == null)
            {
                try
                {
                    await write();
                    return true;
                }
                catch (Exception) { return false; }
            }

            await @this.ExecuteNonQuery(generator.GenerateSavepoint(BEST_EFFORT_SAVEPOINT));

            try
            {
                await write();
            }
            catch (Exception)
            {
                await @this.ExecuteNonQuery(generator.GenerateRollbackToSavepoint(BEST_EFFORT_SAVEPOINT));
                return false;
            }

            var release = generator.GenerateReleaseSavepoint(BEST_EFFORT_SAVEPOINT);
            if (release.HasValue()) await @this.ExecuteNonQuery(release);

            return true;
        }

        /// <summary>
        /// Whether the exception, or one it wraps, is a command that ran out of time on this database. Olive
        /// wraps the driver's exception when a query fails to start, and does not when it fails while being read,
        /// so the whole chain is checked.
        /// </summary>
        public static bool IsTimeout(this IDataAccess @this, Exception exception)
        {
            var generator = @this.GetSqlCommandGenerator() as SqlCommandGenerator;

            for (var e = exception; e != null; e = e.InnerException)
                if (generator?.IsTimeout(e) ?? e is TimeoutException) return true;

            return false;
        }
    }
}
