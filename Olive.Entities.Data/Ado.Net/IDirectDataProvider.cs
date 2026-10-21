namespace Olive.Entities.Data
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    /// <summary>A data provider that can run <see cref="WriteOperations"/>: statements that go straight to the
    /// database, outside Olive's entity pipeline.</summary>
    public interface IDirectDataProvider
    {
        /// <summary>Deletes the rows the query matches in one statement and returns how many.</summary>
        Task<int> DirectDelete(IDatabaseQuery query);

        /// <summary>Updates the rows the query matches in one statement and returns how many.</summary>
        Task<int> DirectUpdate(IDatabaseQuery query, IEnumerable<ColumnAssignment> assignments);

        /// <summary>Takes and updates the rows the query matches in one atomic step and returns them.</summary>
        Task<IEntity[]> DirectClaim(IDatabaseQuery query, IEnumerable<ColumnAssignment> assignments);

        /// <summary>Writes new rows of this provider's entity type, by the database's bulk copy where it can.</summary>
        Task DirectInsert(IEntity[] entities);
    }

    /// <summary>The table a direct command acts on, as the provider's SQL generator spells it.</summary>
    public class DirectCommandTarget
    {
        /// <summary>The table's full name, for the statement's own target (UPDATE x, DELETE FROM x).</summary>
        public string Table { get; set; }

        /// <summary>The table with its alias, as a SELECT's FROM clause has it; the query's criteria and sort
        /// refer to its columns through that alias.</summary>
        public string Tables { get; set; }

        /// <summary>The alias, as written in <see cref="Tables"/>.</summary>
        public string Alias { get; set; }

        /// <summary>The key column, escaped.</summary>
        public string IdColumn { get; set; }
    }

    /// <summary>A SQL generator whose database can copy rows in bulk (SqlBulkCopy, COPY), which
    /// <see cref="WriteOperations.Insert"/> uses when it bypasses the entity pipeline, instead of one INSERT per row.</summary>
    public interface IBulkCopyCommandGenerator
    {
        /// <summary>Writes the rows to the table, columns matched by name, over the connection given: the open
        /// transaction's, with <paramref name="transaction"/>, or a connection of its own with none. Constraints
        /// and triggers apply as they do to an INSERT.</summary>
        Task BulkCopy(DbConnection connection, DbTransaction transaction, string table, DataTable rows);
    }
}
