using Ardalis.Result;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Grid;

public class UserManagementGridService
{
    private readonly IUserRepository _userRepository;

    public UserManagementGridService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GridResult<UserManagementGridModel>>> ExecuteAsync(UserManagementGridQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = await _userRepository.GetQueryableAsync();

        // Apply text search using the new extension
        dbQuery = dbQuery.ApplyTextSearch(query.SearchTerm,
            x => x.FirstName,
            x => x.LastName,
            x => x.Email,
            x => x.Phone);

        // Apply specific filters
        if (!string.IsNullOrEmpty(query.Role))
        {
            dbQuery = dbQuery.Where(x => x.Role == query.Role);
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.EmailConfirmed.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.EmailConfirmed == query.EmailConfirmed.Value);
        }

        if (query.PhoneConfirmed.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.PhoneConfirmed == query.PhoneConfirmed.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.OrganizationId == query.OrganizationId.Value);
        }

        // Date range filters
        if (query.CreatedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        if (query.LastLoginFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.LastLoginAt >= query.LastLoginFrom.Value);
        }

        if (query.LastLoginTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.LastLoginAt <= query.LastLoginTo.Value);
        }

        // Get total count first
        var totalCount = await Task.Run(() => dbQuery.Count(), cancellationToken);

        // Apply sorting and paging on the entity query
        var pagedQuery = dbQuery
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize);

        // Materialize the results
        var entities = await Task.Run(() => pagedQuery.ToList(), cancellationToken);

        // Map to models in memory
        var mappedItems = entities.Select(x => MapToGridModel(x)).ToList();

        var result = new GridResult<UserManagementGridModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<GridResult<UserManagementGridModel>>.Success(result);
    }

    private static UserManagementGridModel MapToGridModel(User user)
    {
        return new UserManagementGridModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneConfirmed,
            LastLoginAt = user.LastLoginAt,
            OrganizationName = null, // Organization relation is not included in the query
            CreatedAt = user.CreatedAt
        };
    }
}