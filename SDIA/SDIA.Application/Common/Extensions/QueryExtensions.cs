using System.Linq.Expressions;
using System.Reflection;
using SDIA.Application.Common.Models;

namespace SDIA.Application.Common.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var validatedPage = Math.Max(1, page);
        var validatedPageSize = Math.Max(1, Math.Min(100, pageSize)); // Limit to 100 max

        return query.Skip((validatedPage - 1) * validatedPageSize).Take(validatedPageSize);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(x => 1); // Default ordering for consistent pagination

        try
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            // Handle nested properties (e.g., "User.Name")
            Expression property = parameter;
            foreach (var prop in sortBy.Split('.'))
            {
                var propertyInfo = property.Type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    // Property not found, return unsorted query with default order
                    return query.OrderBy(x => 1);
                }
                property = Expression.Property(property, propertyInfo);
            }

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
        catch
        {
            // If sorting fails, return unsorted query with default order
            return query.OrderBy(x => 1);
        }
    }

    public static IQueryable<T> ApplyTextSearch<T>(this IQueryable<T> query, string? searchTerm, params Expression<Func<T, string>>[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || !searchProperties.Any())
            return query;

        var trimmedSearchTerm = searchTerm.Trim().ToLower();

        // Build combined search expression
        Expression<Func<T, bool>>? combinedExpression = null;

        foreach (var property in searchProperties)
        {
            var parameter = property.Parameters[0];
            var propertyExpression = property.Body;

            // Create expression for: property.ToLower().Contains(searchTerm)
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            var toLowerCall = Expression.Call(propertyExpression, toLowerMethod!);
            var containsCall = Expression.Call(toLowerCall, containsMethod!, Expression.Constant(trimmedSearchTerm));

            var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);

            combinedExpression = combinedExpression == null
                ? lambda
                : CombineExpressions(combinedExpression, lambda, Expression.OrElse);
        }

        return combinedExpression != null ? query.Where(combinedExpression) : query;
    }

    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        var parameter = Expression.Parameter(typeof(T));
        var left = ReplaceParameter(expr1.Body, expr1.Parameters[0], parameter);
        var right = ReplaceParameter(expr2.Body, expr2.Parameters[0], parameter);
        var combined = combiner(left, right);
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
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