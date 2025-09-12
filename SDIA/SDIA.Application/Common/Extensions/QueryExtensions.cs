using System.Linq.Expressions;
using SDIA.Application.Common.Models;

namespace SDIA.Application.Common.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public static async Task<GridResult<T>> ToGridResultAsync<T>(this IQueryable<T> query, GridQuery<T> gridQuery, CancellationToken cancellationToken = default)
    {
        var totalCount = query.Count();
        
        var data = await Task.FromResult(query
            .ApplySorting(gridQuery.SortBy, gridQuery.SortDescending)
            .ApplyPaging(gridQuery.Page, gridQuery.PageSize)
            .ToList());

        return new GridResult<T>
        {
            Data = data,
            TotalCount = totalCount,
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize
        };
    }
}