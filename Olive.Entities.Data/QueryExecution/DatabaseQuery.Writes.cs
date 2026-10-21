namespace Olive.Entities.Data
{
    partial class DatabaseQuery
    {
        /// <summary>Whether a write that bypasses the entity pipeline passes over rows another transaction holds
        /// rather than waiting for them. See <see cref="WriteOperations.SkipLocked{T}"/>.</summary>
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
    /// Set-based writes: delete, update or insert every row a query matches (or every entity given) in one call.
    /// <para>
    /// By default they go through Olive's entity pipeline, as <c>Database.Delete</c>, <c>Update</c> and <c>Save</c>
    /// do: the matching entities are loaded, then each is validated, raises its events, follows M#'s delete rules
    /// and cascades, is soft-deleted where its type says so, is replicated and audited, all in one transaction.
    /// </para>
    /// <para>
    /// With <c>bypassEntityPipeline: true</c> one statement handles every matching row instead, which is what it
    /// is for: volume and concurrency. It is then OUTSIDE OLIVE'S SCOPE: no validation, no entity events (Saving,
    /// Saved, Deleting, Deleted, and so GlobalEntityEvents), no M# delete rules or cascades, no data replication
    /// (nothing is published to subscribers), no audit trail and no Database.Updated. Instances already in memory
    /// are not marked stale. What it keeps: the type's cache is cleared, at once and again when the open
    /// transaction ends, and the database's own constraints and triggers apply. Entities that span tables
    /// (inheritance) are not supported, nor is a hard delete of a soft-delete entity, and a provider without these
    /// statements throws <see cref="NotSupportedException"/>. <see cref="Claim{TEntity}"/> always works this way.
    /// </para>
    /// <para>
    /// Either way the query's criteria (soft-deleted rows excluded, as in GetList), sort and <c>Top</c> apply.
    /// Paging does not: use <c>Top</c>.
    /// </para>
    /// </summary>
    public static class WriteOperations
    {
        /// <summary>Makes a write that bypasses the entity pipeline pass over rows another transaction holds rather
        /// than wait for them, where the database can. A row passed over is simply not affected this time.
        /// PostgreSQL skips every locked row (SKIP LOCKED); SQL Server (READPAST) only those whose lock it meets on
        /// the index it reads, so on a heap it can still wait for a row reached by a lookup. A write through the
        /// entity pipeline refuses it.</summary>
        public static T SkipLocked<T>(this T query) where T : IDatabaseQuery
        {
            Concrete(query).SkipLockedRows = true;
            return query;
        }

        /// <summary>Deletes every entity the query matches (at most <c>Top</c> of them, if set) and returns how many.</summary>
        /// <param name="bypassEntityPipeline">False (the default): each entity is loaded and deleted by
        /// <c>Database.Delete</c>, in one transaction, with its events, delete rules, cascades, soft delete,
        /// replication and audit. True: one DELETE statement, OUTSIDE OLIVE'S SCOPE (see <see cref="WriteOperations"/>):
        /// none of those happen, and a soft-delete entity is refused.</param>
        public static async Task<int> Delete(this IDatabaseQuery query, bool bypassEntityPipeline = false)
        {
            if (bypassEntityPipeline) return await Provider(query).DirectDelete(query);

            var entities = await ForPipeline(query, "Delete").GetList();
            await Context.Current.Database().Delete(entities);
            return entities.Length;
        }

        /// <summary>Updates every entity the query matches (at most <c>Top</c> of them, if set) and returns how many.</summary>
        /// <param name="bypassEntityPipeline">False (the default): each entity is loaded, changed in memory and saved
        /// by <c>Database.Update</c>, in one transaction, validated, with its events, replication and audit; like
        /// any load-and-save, it can overwrite a change another caller made in between. True: one UPDATE statement
        /// that computes each value from the row as it is in the database, OUTSIDE OLIVE'S SCOPE
        /// (see <see cref="WriteOperations"/>): nothing is validated, raised, replicated or audited.</param>
        public static async Task<int> Update<TEntity>(this IDatabaseQuery<TEntity> query, Action<UpdateSet<TEntity>> set,
            bool bypassEntityPipeline = false)
            where TEntity : IEntity
        {
            var assignments = Assignments(set);
            if (bypassEntityPipeline) return await Provider(query).DirectUpdate(query, assignments);

            ForPipeline(query, "Update");
            var entities = await query.GetList();
            await Context.Current.Database().Update(entities, entity =>
            {
                foreach (var assignment in assignments) assignment.Apply(entity);
            });
            return entities.Length;
        }

        /// <summary>
        /// Takes the rows the query matches (its first <c>Top</c>, in its sort order, if set) and updates them in
        /// one atomic step, so two callers can never claim the same row. With <see cref="SkipLocked{T}"/> a caller
        /// steps over rows another claim holds rather than queue behind it. Returns the claimed entities as they are
        /// after the update.
        /// <para>
        /// ALWAYS OUTSIDE OLIVE'S SCOPE, since loading and saving each entity could not be atomic: the update is
        /// not validated, raises no events and is neither replicated nor audited (see <see cref="WriteOperations"/>).
        /// </para>
        /// </summary>
        public static async Task<TEntity[]> Claim<TEntity>(this IDatabaseQuery<TEntity> query, Action<UpdateSet<TEntity>> set)
            where TEntity : IEntity
            => (await Provider(query).DirectClaim(query, Assignments(set))).Cast<TEntity>().ToArray();

        /// <summary>Inserts new entities.</summary>
        /// <param name="bypassEntityPipeline">False (the default): each is saved by <c>Database.Save</c>, in one
        /// transaction, validated, with its events, replication and audit. True: written by the database's bulk copy
        /// (SqlBulkCopy, PostgreSQL COPY), joining the open DbTransactionScope if there is one, OUTSIDE OLIVE'S SCOPE
        /// (see <see cref="WriteOperations"/>): nothing is validated, raised, replicated or audited. Where a copy
        /// cannot be used (an auto-number to read back, a Blob, inheritance, a System.Transactions scope, or a
        /// provider without one) it falls back to one INSERT per row, still outside the pipeline.</param>
        public static async Task Insert(this IDatabase database, IEnumerable<IEntity> entities, bool bypassEntityPipeline = false)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            var items = entities.ExceptNull().ToArray();
            if (items.Any(e => !e.IsNew)) throw new ArgumentException("Insert() takes new instances only.");

            if (!bypassEntityPipeline)
            {
                await database.Save(items);
                return;
            }

            foreach (var group in items.GroupBy(x => x.GetType()))
            {
                var provider = database.GetProvider(group.Key) as IDirectDataProvider ??
                    throw new NotSupportedException($"{database.GetProvider(group.Key)?.GetType().Name} cannot bypass the entity pipeline.");

                await provider.DirectInsert(group.ToArray());
            }
        }

        static ColumnAssignment[] Assignments<TEntity>(Action<UpdateSet<TEntity>> set) where TEntity : IEntity
        {
            if (set == null) throw new ArgumentNullException(nameof(set));

            var result = new UpdateSet<TEntity>();
            set(result);

            if (result.Assignments.Count == 0) throw new ArgumentException("No column is set.");
            return result.Assignments.ToArray();
        }

        static DatabaseQuery ForPipeline(IDatabaseQuery query, string operation)
        {
            var result = Concrete(query);

            if (result.SkipLockedRows)
                throw new ArgumentException($"SkipLocked() applies to {operation}(bypassEntityPipeline: true) only.");

            if (result.PageSize.HasValue)
                throw new ArgumentException($"{operation}() does not take paging. Use Top() to limit the rows.");

            return result;
        }

        static DatabaseQuery Concrete(IDatabaseQuery query) =>
            query as DatabaseQuery ?? throw new NotSupportedException($"{query?.GetType().Name} does not support set-based writes.");

        static IDirectDataProvider Provider(IDatabaseQuery query)
        {
            Concrete(query);
            return query.Provider as IDirectDataProvider ??
                throw new NotSupportedException($"{query.Provider?.GetType().Name} cannot bypass the entity pipeline.");
        }
    }
}
