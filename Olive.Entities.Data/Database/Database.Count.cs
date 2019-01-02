namespace Olive.Entities.Data
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    partial class Database
    {
        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public Task<int> Count<T>() where T : IEntity => Of<T>().Count();

        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public Task<int> Count<T>(Expression<Func<T, bool>> criteria) where T : IEntity
        {
            return Of<T>().Where(criteria).Count();
        }
    }
}