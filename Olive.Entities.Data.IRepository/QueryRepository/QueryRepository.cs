namespace Olive.Entities.Data.IRepository.GenericRepository;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Olive.Entities.Data.IRepository.GenericRepository.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

internal class QueryRepository<TDbContext> : IQueryRepository, IQueryRepository<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public QueryRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<T> GetQueryable<T>()
        where T : class, IEntity
    {
        return _dbContext.Set<T>();
    }

    public Task<List<T>> GetListAsync<T>(CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        return GetListAsync<T>(false, cancellationToken);
    }

    public Task<List<T>> GetListAsync<T>(bool asTracked, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        Func<IQueryable<T>, IIncludableQueryable<T, object>> nullValue = null;
        return GetListAsync(nullValue, asTracked, cancellationToken);
    }

    public Task<List<T>> GetListAsync<T>(
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        return GetListAsync(includes, false, cancellationToken);
    }

    public async Task<List<T>> GetListAsync<T>(
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        bool asTracked,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (includes != null)
        {
            query = includes(query);
        }

        if (asTracked == false)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return items;
    }

    public Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default)
         where T : class, IEntity
    {
        return GetListAsync(condition, false, cancellationToken);
    }

    public Task<List<T>> GetListAsync<T>(
        Expression<Func<T, bool>> condition,
        bool asTracked,
        CancellationToken cancellationToken = default)
         where T : class, IEntity
    {
        return GetListAsync(condition, null, asTracked, cancellationToken);
    }

    public async Task<List<T>> GetListAsync<T>(
        Expression<Func<T, bool>> condition,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        bool asTracked,
        CancellationToken cancellationToken = default)
         where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (condition != null)
        {
            query = query.Where(condition);
        }

        if (includes != null)
        {
            query = includes(query);
        }

        if (asTracked == false)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return items;
    }

    public Task<List<T>> GetListAsync<T>(Specification<T> specification, CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        return GetListAsync(specification, false, cancellationToken);
    }

    public async Task<List<T>> GetListAsync<T>(
        Specification<T> specification,
        bool asTracked,
        CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (specification != null)
        {
            query = query.GetSpecifiedQuery(specification);
        }

        if (asTracked == false)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        List<TProjectedType> entities = await _dbContext.Set<T>()
            .Select(selectExpression).ToListAsync(cancellationToken).ConfigureAwait(false);

        return entities;
    }

    public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
        Expression<Func<T, bool>> condition,
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        IQueryable<T> query = _dbContext.Set<T>();

        if (condition != null)
        {
            query = query.Where(condition);
        }

        List<TProjectedType> projectedEntites = await query.Select(selectExpression)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return projectedEntites;
    }

    public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
        Specification<T> specification,
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        IQueryable<T> query = _dbContext.Set<T>();

        if (specification != null)
        {
            query = query.GetSpecifiedQuery(specification);
        }

        return await query.Select(selectExpression)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<PaginatedList<T>> GetListAsync<T>(
               PaginationSpecification<T> specification,
               CancellationToken cancellationToken = default)
               where T : class, IEntity
    {
        return GetListAsync(specification, false, cancellationToken);
    }

    public async Task<PaginatedList<T>> GetListAsync<T>(
        PaginationSpecification<T> specification,
        bool asTracked,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        IQueryable<T> query = _dbContext.Set<T>();

        if (asTracked == false)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.ToPaginatedListAsync(specification, cancellationToken); ;
    }

    public async Task<PaginatedList<TProjectedType>> GetListAsync<T, TProjectedType>(
        PaginationSpecification<T> specification,
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
        where TProjectedType : class
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        IQueryable<T> query = _dbContext.Set<T>().GetSpecifiedQuery((SpecificationBase<T>)specification);

        PaginatedList<TProjectedType> paginatedList = await query.Select(selectExpression)
            .ToPaginatedListAsync(specification.PageIndex, specification.PageSize, cancellationToken);
        return paginatedList;
    }

    public Task<T> GetByIdAsync<T>(object id, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetByIdAsync<T>(id, false, cancellationToken);
    }

    public Task<T> GetByIdAsync<T>(object id, bool asTracked, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetByIdAsync<T>(id, null, asTracked, cancellationToken);
    }

    public Task<T> GetByIdAsync<T>(
        object id,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetByIdAsync(id, includes, false, cancellationToken);
    }

    public async Task<T> GetByIdAsync<T>(
        object id,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        bool asTracked = false,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        IEntityType entityType = _dbContext.Model.FindEntityType(typeof(T));

        string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();
        Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

        if (primaryKeyName == null || primaryKeyType == null)
        {
            throw new ArgumentException("Entity does not have any primary key defined", nameof(id));
        }

        object primaryKeyValue = null;

        try
        {
            primaryKeyValue = Convert.ChangeType(id, primaryKeyType, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            throw new ArgumentException($"You can not assign a value of type {id.GetType()} to a property of type {primaryKeyType}");
        }

        ParameterExpression pe = Expression.Parameter(typeof(T), "entity");
        MemberExpression me = Expression.Property(pe, primaryKeyName);
        ConstantExpression constant = Expression.Constant(primaryKeyValue, primaryKeyType);
        BinaryExpression body = Expression.Equal(me, constant);
        Expression<Func<T, bool>> expressionTree = Expression.Lambda<Func<T, bool>>(body, new[] { pe });

        IQueryable<T> query = _dbContext.Set<T>();

        if (includes != null)
        {
            query = includes(query);
        }

        if (!asTracked)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        T entity = await query.FirstOrDefaultAsync(expressionTree, cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<TProjectedType> GetByIdAsync<T, TProjectedType>(
        object id,
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        IEntityType entityType = _dbContext.Model.FindEntityType(typeof(T));

        string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();
        Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

        if (primaryKeyName == null || primaryKeyType == null)
        {
            throw new ArgumentException("Entity does not have any primary key defined", nameof(id));
        }

        object primaryKeyValue = null;

        try
        {
            primaryKeyValue = Convert.ChangeType(id, primaryKeyType, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            throw new ArgumentException($"You can not assign a value of type {id.GetType()} to a property of type {primaryKeyType}");
        }

        ParameterExpression pe = Expression.Parameter(typeof(T), "entity");
        MemberExpression me = Expression.Property(pe, primaryKeyName);
        ConstantExpression constant = Expression.Constant(primaryKeyValue, primaryKeyType);
        BinaryExpression body = Expression.Equal(me, constant);
        Expression<Func<T, bool>> expressionTree = Expression.Lambda<Func<T, bool>>(body, new[] { pe });

        IQueryable<T> query = _dbContext.Set<T>();

        return await query.Where(expressionTree)
                          .Select(selectExpression)
                          .FirstOrDefaultAsync(cancellationToken)
                          .ConfigureAwait(false);
    }

    public Task<T> GetAsync<T>(
        Expression<Func<T, bool>> condition,
        CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        return GetAsync(condition, null, false, cancellationToken);
    }

    public Task<T> GetAsync<T>(
        Expression<Func<T, bool>> condition,
        bool asTracked,
        CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        return GetAsync(condition, null, asTracked, cancellationToken);
    }

    public Task<T> GetAsync<T>(
        Expression<Func<T, bool>> condition,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        return GetAsync(condition, includes, false, cancellationToken);
    }

    public async Task<T> GetAsync<T>(
        Expression<Func<T, bool>> condition,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
        bool asTracked,
        CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (condition != null)
        {
            query = query.Where(condition);
        }

        if (includes != null)
        {
            query = includes(query);
        }

        if (asTracked == false)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<T> GetAsync<T>(Specification<T> specification, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        return GetAsync(specification, false, cancellationToken);
    }

    public async Task<T> GetAsync<T>(Specification<T> specification, bool asTracked, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (specification != null)
        {
            query = query.GetSpecifiedQuery(specification);
        }

        if (asTracked == false)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TProjectedType> GetAsync<T, TProjectedType>(
        Expression<Func<T, bool>> condition,
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        IQueryable<T> query = _dbContext.Set<T>();

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TProjectedType> GetAsync<T, TProjectedType>(
        Specification<T> specification,
        Expression<Func<T, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (selectExpression == null)
        {
            throw new ArgumentNullException(nameof(selectExpression));
        }

        IQueryable<T> query = _dbContext.Set<T>();

        if (specification != null)
        {
            query = query.GetSpecifiedQuery(specification);
        }

        return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> ExistsAsync<T>(CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        return ExistsAsync<T>(null, cancellationToken);
    }

    public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (condition == null)
        {
            return await query.AnyAsync(cancellationToken);
        }

        bool isExists = await query.AnyAsync(condition, cancellationToken).ConfigureAwait(false);
        return isExists;
    }

    public async Task<bool> ExistsByIdAsync<T>(object id, CancellationToken cancellationToken = default)
       where T : class, IEntity
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        IEntityType entityType = _dbContext.Model.FindEntityType(typeof(T));

        string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();
        Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

        if (primaryKeyName == null || primaryKeyType == null)
        {
            throw new ArgumentException("Entity does not have any primary key defined", nameof(id));
        }

        object primaryKeyValue = null;

        try
        {
            primaryKeyValue = Convert.ChangeType(id, primaryKeyType, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            throw new ArgumentException($"You can not assign a value of type {id.GetType()} to a property of type {primaryKeyType}");
        }

        ParameterExpression pe = Expression.Parameter(typeof(T), "entity");
        MemberExpression me = Expression.Property(pe, primaryKeyName);
        ConstantExpression constant = Expression.Constant(primaryKeyValue, primaryKeyType);
        BinaryExpression body = Expression.Equal(me, constant);
        Expression<Func<T, bool>> expressionTree = Expression.Lambda<Func<T, bool>>(body, new[] { pe });

        IQueryable<T> query = _dbContext.Set<T>();

        bool isExistent = await query.AnyAsync(expressionTree, cancellationToken).ConfigureAwait(false);
        return isExistent;
    }

    public async Task<int> GetCountAsync<T>(CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        int count = await _dbContext.Set<T>().CountAsync(cancellationToken).ConfigureAwait(false);
        return count;
    }

    public async Task<int> GetCountAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> GetCountAsync<T>(IEnumerable<Expression<Func<T, bool>>> conditions, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (conditions != null)
        {
            foreach (Expression<Func<T, bool>> expression in conditions)
            {
                query = query.Where(expression);
            }
        }

        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<long> GetLongCountAsync<T>(CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        long count = await _dbContext.Set<T>().LongCountAsync(cancellationToken).ConfigureAwait(false);
        return count;
    }

    public async Task<long> GetLongCountAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<long> GetLongCountAsync<T>(IEnumerable<Expression<Func<T, bool>>> conditions, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (conditions != null)
        {
            foreach (Expression<Func<T, bool>> expression in conditions)
            {
                query = query.Where(expression);
            }
        }

        return await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
    }

    // DbContext level members
    public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }

        IEnumerable<object> parameters = new List<object>();

        List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
        return items;
    }

    public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, object parameter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }

        List<object> parameters = new List<object>() { parameter };
        List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
        return items;
    }

    public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<DbParameter> parameters, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }

        List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
        return items;
    }

    public async Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }

        List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
        return items;
    }
}
