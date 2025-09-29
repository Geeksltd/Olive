namespace Olive.Entities.Data.IRepository.GenericRepository;

using Olive.Entities.Data.IRepository.GenericRepository.Entities;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

internal static class SpecificationEvaluator
{
    public static IQueryable<T> GetSpecifiedQuery<T>(this IQueryable<T> inputQuery, Specification<T> specification)
        where T : IEntity
    {
        IQueryable<T> query = GetSpecifiedQuery(inputQuery, (SpecificationBase<T>)specification);

        // Apply paging if enabled
        if (specification.Skip != null)
        {
            if (specification.Skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(specification), $"The value of {nameof(specification.Skip)} in {nameof(specification)} can not be negative.");
            }

            query = query.Skip((int)specification.Skip);
        }

        if (specification.Take != null)
        {
            if (specification.Take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(specification), $"The value of {nameof(specification.Take)} in {nameof(specification)} can not be negative.");
            }

            query = query.Take((int)specification.Take);
        }

        return query;
    }

    public static IQueryable<T> GetSpecifiedQuery<T>(this IQueryable<T> inputQuery, PaginationSpecification<T> specification)
        where T : IEntity
    {
        if (inputQuery == null)
        {
            throw new ArgumentNullException(nameof(inputQuery));
        }

        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        if (specification.PageIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(specification), "The value of specification.PageIndex must be greater than 0.");
        }

        if (specification.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(specification), "The value of specification.PageSize must be greater than 0.");
        }

        IQueryable<T> query = GetSpecifiedQuery(inputQuery, (SpecificationBase<T>)specification);

        // Apply paging if enabled
        int skip = (specification.PageIndex - 1) * specification.PageSize;

        query = query.Skip(skip).Take(specification.PageSize);

        return query;
    }

    public static IQueryable<T> GetSpecifiedQuery<T>(this IQueryable<T> inputQuery, SpecificationBase<T> specification)
        where T : IEntity
    {
        if (inputQuery == null)
        {
            throw new ArgumentNullException(nameof(inputQuery));
        }

        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        IQueryable<T> query = inputQuery;

        if (typeof(IArchivableEntity).IsAssignableFrom(typeof(T)))
        {
            query = specification.StateFilter switch
            {
                StateFilter.Active =>
                    query.Where(BuildIsArchivedPredicate<T>(false)),
                StateFilter.Archived =>
                    query.Where(BuildIsArchivedPredicate<T>(true)),
                _ => query,
            };
        }

        // modify the IQueryable using the specification's criteria expression
        if (specification.Conditions != null && specification.Conditions.Count != 0)
        {
            foreach (Expression<Func<T, bool>> specificationCondition in specification.Conditions)
            {
                query = query.Where(specificationCondition);
            }
        }


        // Includes all expression-based includes
        if (specification.Includes != null)
        {
            query = specification.Includes(query);
        }

        // Apply ordering if expressions are set
        if (specification.OrderBy != null)
        {
            query = specification.OrderBy(query);
        }
        else if (!string.IsNullOrWhiteSpace(specification.OrderByDynamic.ColumnName) && !string.IsNullOrWhiteSpace(specification.OrderByDynamic.SortDirection))
        {
            query = query.OrderBy(specification.OrderByDynamic.ColumnName + " " + specification.OrderByDynamic.SortDirection);
        }

        return query;
    }

    private static Expression<Func<T, bool>> BuildIsArchivedPredicate<T>(bool isArchived)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var isArchivedProperty = Expression.Property(parameter, nameof(IArchivableEntity.IsArchived));
        var condition = Expression.Equal(isArchivedProperty, Expression.Constant(isArchived));
        return Expression.Lambda<Func<T, bool>>(condition, parameter);
    }
}
