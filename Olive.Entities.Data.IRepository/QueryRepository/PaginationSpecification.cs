namespace Olive.Entities.Data.IRepository.GenericRepository;

using Olive.Entities.Data.IRepository.GenericRepository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This object holds the pagination query specifications.
/// </summary>
/// <typeparam name="T">The database entity i.e an <see cref="DbSet{TEntity}"/> object.</typeparam>
public class PaginationSpecification<T> : SpecificationBase<T>
    where T : IEntity
{
    /// <summary>
    /// Gets or sets the current page index.
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }
}
