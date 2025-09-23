using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Mappings;
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
        var dbQuery = _userRepository.GetAll(cancellationToken);

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
        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Apply projection, sorting and paging
        var mappedQuery = dbQuery
            .Select(UserMappings.ToGridModel())
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize);

        // Execute query with projection
        var mappedItems = await mappedQuery.ToListAsync(cancellationToken);

        var result = new GridResult<UserManagementGridModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<GridResult<UserManagementGridModel>>.Success(result);
    }
}