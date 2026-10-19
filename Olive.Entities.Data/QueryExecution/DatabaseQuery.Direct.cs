namespace Olive.Entities.Data
{
    partial class DatabaseQuery
    {
        /// <summary>Whether a direct operation passes over rows another transaction holds rather than waiting
        /// for them. See <see cref="DirectOperations.SkipLocked{T}"/>.</summary>
        public bool SkipLockedRows { get; set; }
    }
}

namespace Olive.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive.Entities.Data;

    /// <summary>
    /// Direct operations: statements that go straight to the database, outside Olive's entity pipeline. One
    /// statement handles every matching row, instead of loading them and saving or deleting each, which is
    /// what they are for: volume and concurrency.
    /// <para>
    /// OUTSIDE OLIVE'S SCOPE. They skip everything a save or delete does per entity: validation, the entity
    /// events (Saving, Saved, Deleting, Deleted, and so GlobalEntityEvents), M#'s delete rules and cascades,
    /// data replication (nothing is published to subscribers), the audit trail and Database.Updated. Instances
    /// already in memory are not marked stale. What they do keep: the type's cache is cleared, at once and again
    /// when the open transaction ends, and the database's own constraints and triggers apply.
    /// </para>
    /// <para>
    /// The query's criteria (soft-deleted rows excluded, as in GetList), sort and <c>Top</c> apply. Paging does
    /// not, and neither does Include. Entities that span tables (inheritance) are not supported, nor is a hard
    /// delete of a soft-delete entity. A provider without these operations throws <see cref="NotSupportedException"/>.
    /// </para>
    /// </summary>
    public static class DirectOperations
    {
        /// <summary>Makes the direct operation pass over rows another transaction holds rather than wait for
        /// them, where the database can. A row passed over is simply not affected this time. PostgreSQL skips
        /// every locked row (SKIP LOCKED); SQL Server (READPAST) only those whose lock it meets on the index
        /// it reads, so on a heap it can still wait for a row reached by a lookup.</summary>
        public static T SkipLocked<T>(this T query) where T : IDatabaseQuery
        {
            Concrete(query).SkipLockedRows = true;
            return query;
        }

        /// <summary>Deletes every row the query matches (at most <c>Top</c> of them, if set) in one statement,
        /// and returns how many. Outside Olive's scope: see <see cref="DirectOperations"/>.</summary>
        public static Task<int> DirectDelete(this IDatabaseQuery query) => Provider(query).DirectDelete(query);

        /// <summary>Updates every row the query matches (at most <c>Top</c> of them, if set) in one statement,
        /// and returns how many. Outside Olive's scope: see <see cref="DirectOperations"/>.</summary>
        public static Task<int> DirectUpdate<TEntity>(this IDatabaseQuery<TEntity> query, Action<DirectUpdateSet<TEntity>> set)
            where TEntity : IEntity
            => Provider(query).DirectUpdate(query, Assignments(set));

        /// <summary>
        /// Takes the rows the query matches (its first <c>Top</c>, in its sort order, if set) and updates them
        /// in one atomic step, so two callers can never claim the same row. With <see cref="SkipLocked{T}"/> a
        /// caller steps over rows another claim holds rather than queue behind it. Returns the claimed entities
        /// as they are after the update. Outside Olive's scope: see <see cref="DirectOperations"/>.
        /// </summary>
        public static async Task<TEntity[]> DirectClaim<TEntity>(this IDatabaseQuery<TEntity> query, Action<DirectUpdateSet<TEntity>> set)
            where TEntity : IEntity
            => (await Provider(query).DirectClaim(query, Assignments(set))).Cast<TEntity>().ToArray();

        /// <summary>
        /// Inserts new entities by the database's bulk copy (SqlBulkCopy, PostgreSQL COPY), joining the open
        /// DbTransactionScope if there is one. Where a copy cannot be used (an auto-number to read back, a Blob,
        /// inheritance, a System.Transactions scope, or a provider without one) it falls back to one INSERT per
        /// row. Either way, outside Olive's scope: see <see cref="DirectOperations"/>; unlike
        /// <see cref="IDatabase.BulkInsert"/> nothing is validated.
        /// </summary>
        public static async Task DirectInsert(this IDatabase database, IEnumerable<IEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var group in entities.ExceptNull().GroupBy(x => x.GetType()))
            {
                var provider = database.GetProvider(group.Key) as IDirectDataProvider ??
                    throw new NotSupportedException($"{database.GetProvider(group.Key)?.GetType().Name} does not support direct operations.");

                await provider.DirectInsert(group.ToArray());
            }
        }

        static DirectAssignment[] Assignments<TEntity>(Action<DirectUpdateSet<TEntity>> set) where TEntity : IEntity
        {
            if (set == null) throw new ArgumentNullException(nameof(set));

            var result = new DirectUpdateSet<TEntity>();
            set(result);

            if (result.Assignments.Count == 0) throw new ArgumentException("No column is set.");
            return result.Assignments.ToArray();
        }

        static DatabaseQuery Concrete(IDatabaseQuery query) =>
            query as DatabaseQuery ?? throw new NotSupportedException($"{query?.GetType().Name} does not support direct operations.");

        static IDirectDataProvider Provider(IDatabaseQuery query)
        {
            Concrete(query);
            return query.Provider as IDirectDataProvider ??
                throw new NotSupportedException($"{query.Provider?.GetType().Name} does not support direct operations.");
        }
    }
}
