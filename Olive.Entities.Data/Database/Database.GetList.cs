using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class Database
    {
        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public Task<T[]> GetList<T>(Expression<Func<T, bool>> criteria = null) where T : IEntity
        {
            return Of<T>().Where(criteria).GetList();
        }
    }
}