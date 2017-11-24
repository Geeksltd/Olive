using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class Database
    {
        /// <summary>
        /// Saves the specified records in the data repository.
        /// The operation will run in a Transaction.
        /// </summary>
        public Task<IEnumerable<T>> Save<T>(T[] records) where T : IEntity => Save(records as IEnumerable<T>);

        /// <summary>
        /// Saves the specified records in the data repository.
        /// The operation will run in a Transaction.
        /// </summary>
        public Task<IEnumerable<T>> Save<T>(IEnumerable<T> records) where T : IEntity
            => Save<T>(records, SaveBehaviour.Default);

        /// <summary>
        /// Saves the specified records in the data repository.
        /// The operation will run in a Transaction.
        /// </summary>
        public async Task<IEnumerable<T>> Save<T>(IEnumerable<T> records, SaveBehaviour behaviour) where T : IEntity
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            if (records.None()) return records;

            await EnlistOrCreateTransaction(async () =>
            {
                foreach (var record in records)
                    await Save(record as Entity, behaviour);
            });

            return records;
        }
    }
}