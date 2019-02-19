namespace Olive.Entities.Data
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    partial class Database
    {
        /// <summary>
        /// Finds an object with the specified type matching the specified criteria.
        /// If not found, it returns null.
        /// </summary>
        public Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> criteria) where T : IEntity
        {
            return Of<T>().Where(criteria).FirstOrDefault();
        }

        /// <summary>
        /// Finds an object with the specified type matching the specified criteria.
        /// If not found, it returns null.
        /// </summary>
        public Task<T> FirstOrDefault<T>() where T : IEntity => Of<T>().FirstOrDefault();
    }
}