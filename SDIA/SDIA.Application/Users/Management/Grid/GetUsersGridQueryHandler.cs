using Ardalis.Result;
using MediatR;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Grid;

public class GetUsersGridQueryHandler : IRequestHandler<GetUsersGridQuery, Result<GridResult<UserGridDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersGridQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GridResult<UserGridDto>>> Handle(GetUsersGridQuery request, CancellationToken cancellationToken)
    {
        var query = await _userRepository.GetQueryableAsync();
        
        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.FirstName.ToLower().Contains(searchTerm) ||
                x.LastName.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm) ||
                x.Phone.Contains(searchTerm));
        }
        
        if (!string.IsNullOrEmpty(request.Role))
        {
            query = query.Where(x => x.Role == request.Role);
        }
        
        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }
        
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == request.OrganizationId.Value);
        }

        // Map to DTOs
        var mappedQuery = query.Select(x => MapToUserGridDto(x));
        
        var result = await mappedQuery.ToGridResultAsync(request, cancellationToken);
        
        return Result<GridResult<UserGridDto>>.Success(result);
    }

    private static UserGridDto MapToUserGridDto(User user)
    {
        return new UserGridDto
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
            OrganizationName = user.Organization != null ? user.Organization.Name : null,
            CreatedAt = user.CreatedAt
        };
    }
}