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

        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(x =>
                x.FirstName.ToLower().Contains(searchTerm) ||
                x.LastName.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm) ||
                x.Phone.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(query.Role))
        {
            dbQuery = dbQuery.Where(x => x.Role == query.Role);
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.OrganizationId == query.OrganizationId.Value);
        }

        // Map to models
        var mappedQuery = dbQuery.Select(x => MapToGridModel(x));

        var result = await mappedQuery.ToGridResultAsync(query, cancellationToken);

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